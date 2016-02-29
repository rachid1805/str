using System;
using System.Collections.Generic;
using System.Linq;
using System.IO;
using System.Reflection;
using System.Xml;
using Common.Entities;

namespace PatientGenerator
{
  public class PatientGenerator : IPatientGenerator
  {
    #region Constants

    private const string DocumentTag = "Document";
    private const string DocumentTitleAttribName = "Title";
    private const string DocumentVersionAttribName = "Version";

    private const string HospitalsTag = "Hospitals";
    private const string HospitalTag = "Hospital";
    private const string NameTag = "Name";
    private const string DoctorsTag = "Doctors";
    private const string HostpitalIdsFile = "HospitalIds.xml";

    private const string DiseasesTag = "Diseases";
    private const string DiseaseTag = "Disease";
    private const string TypeTag = "Type";
    private const string PriorityTag = "Priority";
    private const string RequiredTimeValueTag = "RequiredTimeValue";
    private const string RequiredTimeUnitTag = "RequiredTimeUnit";
    private const string DiseaseIdsFile = "DiseaseIds.xml";

    #endregion

    #region Attributes

    private readonly IDictionary<string, int> _hospitals;
    private readonly IDictionary<string, IList<string>> _freeDoctors;
    private readonly IDictionary<string, IList<string>> _busyDoctors;
    private readonly IDictionary<int, IPatientArrival> _patientsArrival;
    private readonly IDictionary<int, IPatientCare> _patientsCare;
    private readonly IDictionary<int, IPatientLeaving> _patientsLeaving;
    private readonly IList<IDisease> _diseases;

    #endregion

    #region Constructor

    public PatientGenerator()
    {
      _hospitals = new Dictionary<string, int>();
      _freeDoctors = new Dictionary<string, IList<string>>();
      _busyDoctors = new Dictionary<string, IList<string>>();
      _patientsArrival = new Dictionary<int, IPatientArrival>();
      _patientsCare = new Dictionary<int, IPatientCare>();
      _patientsLeaving = new Dictionary<int, IPatientLeaving>();
      _diseases = new List<IDisease>();

      var xmlFileDir = Path.GetDirectoryName(new Uri(Assembly.GetExecutingAssembly().CodeBase).LocalPath);
      if (xmlFileDir == null)
      {
        throw new DirectoryNotFoundException("Could not find the specified directory");
      }

      // Populates Hostpitals from the xml file
      var xmlFilepath = Path.Combine(xmlFileDir, HostpitalIdsFile);
      ReadXmlFile(xmlFilepath);

      // Populates Diseases from the xml file
      xmlFilepath = Path.Combine(xmlFileDir, DiseaseIdsFile);
      ReadXmlFile(xmlFilepath);

      // Assign doctors to each hospital
      foreach (var hospital in _hospitals)
      {
        var numberOfDoctors = hospital.Value;
        var doctorsList = new List<string>(numberOfDoctors);
        for (var i = 0; i < numberOfDoctors; ++i)
        {
          doctorsList.Add(GeneratorHelper.RandomUpperChars(3) + GeneratorHelper.RandomNumericalChars(3));
        }
        _freeDoctors.Add(hospital.Key, doctorsList);
        _busyDoctors.Add(hospital.Key, new List<string>());
      }
    }

    #endregion

    #region IPatientLeaving implementation

    public IPatientArrival GeneratePatientArrival()
    {
      var patientId = GeneratorHelper.RandomNumericalValue(1000);

      while (_patientsArrival.Keys.Contains(patientId))
      {
        patientId = GeneratorHelper.RandomNumericalValue(1000);
      }

      var patientArrival = new PatientArrival(GeneratorHelper.RandomUpperChars(4) + GeneratorHelper.RandomNumericalChars(8),
                                              _hospitals.Keys.ToList()[GeneratorHelper.RandomNumericalValue(_hospitals.Count)],
                                              _diseases[GeneratorHelper.RandomNumericalValue(_diseases.Count)],
                                              DateTime.Now);

      // Keep the new generated patient in the arrival list
      _patientsArrival.Add(patientId, patientArrival);

      return patientArrival;
    }

    public IPatientCare GeneratePatientCare()
    {
      var atLeastOneFreeDoctor = false;
      if (_patientsArrival.Count == 0)
      {
        throw new ApplicationException("No patient in the arrival list");
      }
      foreach (var doctor in _freeDoctors)
      {
        if (doctor.Value.Count != 0)
        {
          atLeastOneFreeDoctor = true;
          break;
        }
      }
      if (!atLeastOneFreeDoctor)
      {
        throw new ApplicationException("No doctor available");
      }

      IPatientCare patientCare = null;
      var freeDoctor = false;

      while (!freeDoctor)
      {
        // Take a patient from the waiting list
        var patientId = _patientsArrival.Keys.ToList()[GeneratorHelper.RandomNumericalValue(_patientsArrival.Count)];
        var patientArrival = _patientsArrival[patientId];
        var freeDoctorsForThisHospital = _freeDoctors[patientArrival.HospitalId];

        if (freeDoctorsForThisHospital.Count != 0)
        {
          // Choose one free doctor
          var doctorId = freeDoctorsForThisHospital[GeneratorHelper.RandomNumericalValue(freeDoctorsForThisHospital.Count)];

          // Add this doctor to the busy list
          _busyDoctors[patientArrival.HospitalId].Add(doctorId);

          // Remove this doctor from the free list
          _freeDoctors[patientArrival.HospitalId].Remove(doctorId);

          // Keep the new generated patient in the care list
          patientCare = new PatientCare(patientArrival.PatientId, patientArrival.HospitalId, DateTime.Now, doctorId);
          _patientsCare.Add(patientId, patientCare);

          // Remove this patient from the arrival list
          _patientsArrival.Remove(patientId);

          freeDoctor = true;
        }
      }

      return patientCare;
    }

    public IPatientLeaving GeneratePatientLeaving()
    {
      if (_patientsCare.Count == 0)
      {
        throw new ApplicationException("No patient in the care list");
      }

      // Take a patient from the care list
      var patientId = _patientsCare.Keys.ToList()[GeneratorHelper.RandomNumericalValue(_patientsCare.Count)];
      var patientCare = _patientsCare[patientId];

      // Remove this doctor from the busy list
      _freeDoctors[patientCare.HospitalId].Remove(patientCare.DoctorId);

      // Add this doctor to the free list
      _freeDoctors[patientCare.HospitalId].Add(patientCare.DoctorId);

      // Keep the new generated patient in the leaving list
      var patientLeaving = new PatientLeaving(patientCare.PatientId, patientCare.HospitalId, DateTime.Now);
      _patientsLeaving.Add(patientId, patientLeaving);

      // Remove this patient from the care list
      _patientsCare.Remove(patientId);

      return patientLeaving;
    }

    #endregion

    #region Private Functions

    private void ReadXmlFile(string xmlFilepath)
    {
      if (!File.Exists(xmlFilepath))
      {
        throw new FileNotFoundException(string.Format("Could not find file: {0}", xmlFilepath));
      }

      // First, try to create an XmlReader for the specified file path.
      using (var xmlReader = XmlReader.Create(xmlFilepath))
      {
        var documentTagFound = false;
        var hospitalsTagFound = false;
        var hospitalTagFound = false;
        var diseasesTagFound = false;
        var diseaseTagFound = false;
        var docTitle = string.Empty;
        var docVersion = string.Empty;

        // Parse the file and collect required attributes and values. 
        while (xmlReader.Read())
        {
          if (xmlReader.IsStartElement())
          {
            switch (xmlReader.Name)
            {
              case DocumentTag:
                documentTagFound = true;
                // if an attribute is not found, the value will be Null
                docTitle = xmlReader[DocumentTitleAttribName];
                docVersion = xmlReader[DocumentVersionAttribName];
                break;
              case HospitalsTag:
                hospitalsTagFound = true;
                break;
              case DiseasesTag:
                diseasesTagFound = true;
                break;
              case HospitalTag:
                hospitalTagFound = true;
                ReadHospitalId(xmlReader);
                break;
              case DiseaseTag:
                diseaseTagFound = true;
                ReadDiseaseId(xmlReader);
                break;
            }
          }
        }

        // Validate the information retrieved and throw if missing or invalid.
        if (!documentTagFound)
        {
          throw new ApplicationException("Unrecognized XML file format provided: No Document Tag Found!");
        }

        if (string.IsNullOrEmpty(docTitle))
        {
          throw new ApplicationException("Unrecognized XML file format provided: Document Tag missing Title attribute!");
        }

        if (string.IsNullOrEmpty(docVersion))
        {
          throw new ApplicationException("Unrecognized XML file format provided: Document Tag missing Version attribute!");
        }

        if (!hospitalsTagFound && !diseasesTagFound)
        {
          throw new ApplicationException("Unrecognized XML file format provided: Hospitals/Diseases Tag Not Found!");
        }

        if (!hospitalTagFound && !diseaseTagFound)
        {
          throw new ApplicationException("Unrecognized XML file format provided: Hospital/Disease Tag Not Found!");
        }
      }
    }

    private void ReadHospitalId(XmlReader xmlReader)
    {
      // Read Name attribute
      var name = xmlReader[NameTag];
      if (name == null)
      {
        throw new ApplicationException("Unrecognized XML file format provided: Name Tag Not Found!!");
      }

      // Read Doctors attribute
      var nbOfDoctorsStr = xmlReader[DoctorsTag];
      if (nbOfDoctorsStr == null)
      {
        throw new ApplicationException("Unrecognized XML file format provided: Doctors Tag Not Found!!");
      }

      _hospitals.Add(name, int.Parse(nbOfDoctorsStr));
    }

    private void ReadDiseaseId(XmlReader xmlReader)
    {
      // Read Type attribute
      var type = xmlReader[TypeTag];
      if (type == null)
      {
        throw new ApplicationException("Unrecognized XML file format provided: Type Tag Not Found!!");
      }

      // Read Priority attribute
      var priorityStr = xmlReader[PriorityTag];
      if (priorityStr == null)
      {
        throw new ApplicationException("Unrecognized XML file format provided: Priority Tag Not Found!!");
      }

      // Read RequiredTimeValue attribute
      var requiredTimeValueStr = xmlReader[RequiredTimeValueTag];
      if (requiredTimeValueStr == null)
      {
        throw new ApplicationException("Unrecognized XML file format provided: RequiredTimeValue Tag Not Found!!");
      }

      // Read RequiredTimeUnit attribute
      var requiredTimeUnitStr = xmlReader[RequiredTimeUnitTag];
      if (requiredTimeUnitStr == null)
      {
        throw new ApplicationException("Unrecognized XML file format provided: RequiredTimeUnit Tag Not Found!!");
      }

      _diseases.Add(new Disease(ParseEnum<DiseaseType>(type),
                                ParseEnum<DiseasePriority>(priorityStr),
                                uint.Parse(requiredTimeValueStr),
                                ParseEnum<RequiredTimeUnit>(requiredTimeUnitStr)));
    }

    private static T ParseEnum<T>(string value)
    {
      return (T)Enum.Parse(typeof(T), value, true);
    }

    #endregion
  }
}
