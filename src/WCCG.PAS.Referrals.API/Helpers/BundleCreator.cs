using System.Globalization;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.API.Configuration;
using WCCG.PAS.Referrals.API.DbModels;

namespace WCCG.PAS.Referrals.API.Helpers;

public class BundleCreator : IBundleCreator
{
    private readonly Dictionary<string, RequestPriority> _requestPriorityDictionary = new(StringComparer.OrdinalIgnoreCase)
    {
        { "U", RequestPriority.Urgent },
        { "A", RequestPriority.Asap },
        { "R", RequestPriority.Routine },
        { "S", RequestPriority.Stat },
    };

    private readonly Dictionary<string, Timing.UnitsOfTime> _frequencyDictionary = new(StringComparer.OrdinalIgnoreCase)
    {
        { "A", Timing.UnitsOfTime.A },
        { "D", Timing.UnitsOfTime.D },
        { "H", Timing.UnitsOfTime.H },
        { "M", Timing.UnitsOfTime.Mo },
        { "S", Timing.UnitsOfTime.S },
        { "W", Timing.UnitsOfTime.Wk },
    };

    private readonly DateTimeOffset _currentDate = DateTimeOffset.UtcNow;
    private readonly string _messageHeaderId = Guid.NewGuid().ToString();
    private readonly string _serviceRequestId = Guid.NewGuid().ToString();
    private readonly string _patientId = Guid.NewGuid().ToString();
    private readonly string _practitionerReceivingClinicianId = Guid.NewGuid().ToString();
    private readonly string _practitionerRequestingPractitionerId = Guid.NewGuid().ToString();
    private readonly string _organizationDhaId = Guid.NewGuid().ToString();
    private readonly string _organizationReferringPracticeId = Guid.NewGuid().ToString();
    private readonly string _organizationDestinationId = Guid.NewGuid().ToString();
    private readonly string _encounterRequesterId = Guid.NewGuid().ToString();
    private readonly string _appointmentRequesterId = Guid.NewGuid().ToString();
    private readonly string _encounterPlannedId = Guid.NewGuid().ToString();
    private readonly string _appointmentPlannedId = Guid.NewGuid().ToString();
    private readonly string _carePlanId = Guid.NewGuid().ToString();

    private ReferralDbModel? _referralDbModel;

    private readonly BundleCreationConfig _bundleCreationConfig;

    public BundleCreator(IOptions<BundleCreationConfig> bundleCreationConfig)
    {
        _bundleCreationConfig = bundleCreationConfig.Value;
    }

    public Bundle CreateBundle(ReferralDbModel referralDbModel)
    {
        _referralDbModel = referralDbModel;
        return new Bundle
        {
            Timestamp = new DateTimeOffset(DateTime.UtcNow),
            Type = Bundle.BundleType.Message,
            Entry =
            [
                GenerateMessageHeader(),
                GenerateServiceRequest(),
                GeneratePatient(),
                GeneratePractitioner_ReceivingClinician(),
                GeneratePractitioner_RequestingPractitioner(),
                GenerateOrganization_Dha(),
                GenerateOrganization_ReferringPractice(),
                GenerateOrganization_Destination(),
                GenerateEncounter_Requester(),
                GenerateAppointment_Requester(),
                GenerateCarePlan(),
                GenerateEncounter_Planned(),
                GenerateAppointment_Planned()
            ]
        };
    }

    private Bundle.EntryComponent GenerateMessageHeader()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_messageHeaderId}",
            Resource = new MessageHeader
            {
                Event = new Coding
                {
                    System = "https://fhir.nhs.uk/CodeSystem/message-events-bars",
                    Code = "servicerequest-request"
                },
                Destination =
                [
                    new MessageHeader.MessageDestinationComponent
                    {
                        Receiver = new ResourceReference($"urn:uuid:{_organizationDestinationId}"),
                        Endpoint = _bundleCreationConfig.EReferralsBaseUrl + _bundleCreationConfig.EReferralsCreateReferralEndpoint
                    }
                ],
                Sender = new ResourceReference($"urn:uuid:{_organizationReferringPracticeId}"),
                Source = new MessageHeader.MessageSourceComponent { Endpoint = _bundleCreationConfig.DentalUiBaseUrl },
                Reason = new CodeableConcept(
                    system: "https://fhir.nhs.uk/CodeSystem/message-reason-bars",
                    code: "new"
                ),
                Focus = [new ResourceReference($"urn:uuid:{_serviceRequestId}")],
                Definition = "https://fhir.nhs.uk/MessageDefinition/bars-message-servicerequest-request-referral"
            }
        };
    }

    private Bundle.EntryComponent GenerateServiceRequest()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_serviceRequestId}",
            Resource = new ServiceRequest
            {
                Meta = new Meta { Profile = ["https://fhir.nhs.uk/StructureDefinition/BARSServiceRequest-request-referral"] },
                Identifier =
                [
                    new Identifier
                    {
                        System = "dhcw/LinkId",
                        Value = $"REF{_currentDate.ToString("yyyyMMddHHmm", CultureInfo.InvariantCulture)}"
                    },

                    new Identifier
                    {
                        System = "ReferralUniqueId",
                        Value = _referralDbModel!.ReferralId
                    }
                ],
                Category =
                [
                    new CodeableConcept().Add(
                        system: "https://fhir.nhs.uk/CodeSystem/message-category-servicerequest",
                        code: "referral",
                        display: "Transfer of Care")
                ],
                BasedOn =
                [
                    new ResourceReference($"urn:uuid:{_carePlanId}")
                ],
                Status = RequestStatus.Active,
                Intent = RequestIntent.Plan,
                Priority = _requestPriorityDictionary[_referralDbModel.Priority!],
                Subject = new ResourceReference($"urn:uuid:{_patientId}"),
                AuthoredOn = _referralDbModel.CreationDate!,
                Requester = new ResourceReference($"urn:uuid:{_practitionerRequestingPractitionerId}"),
                Performer = [new ResourceReference($"urn:uuid:{_organizationDhaId}")],
                OrderDetail =
                [
                    new CodeableConcept(system: "dhcw/WlistCodes", code: _referralDbModel.WaitingList!),
                    new CodeableConcept(system: "dhcw/IntentReferValues", code: _referralDbModel.IntendedManagement!),
                    new CodeableConcept(system: "dhcw/patientCategory", code: _referralDbModel.PatientCategory!),
                    new CodeableConcept(system: "dhcw/Datonsys", code: _referralDbModel.HealthBoardReceiveDate!),
                    new CodeableConcept(system: "dhcw/optom/hrf", code: _referralDbModel.HealthRiskFactor!)
                ],
                Encounter = new ResourceReference($"urn:uuid:{_encounterRequesterId}"),
                Occurrence = new Timing
                {
                    Event = [_referralDbModel.FirstAppointmentDate],
                    Repeat = new Timing.RepeatComponent
                    {
                        Period = decimal.Parse(_referralDbModel.RepeatPeriod![..1], CultureInfo.InvariantCulture),
                        PeriodUnit = _frequencyDictionary[_referralDbModel.RepeatPeriod!.Substring(1, 1)]
                    }
                },
                LocationCode =
                [
                    new CodeableConcept(
                        system: "https://fhir.nhs.uk/Id/ods-organization-code",
                        code: _referralDbModel.ReferralAssignedLocation!)
                ],
                Extension =
                [
                    new Extension
                    {
                        Url = "https://fhir.hl7.org.uk/StructureDefinition/Extension-UKCore-SourceOfServiceRequest",
                        Value = new CodeableConcept().Add(
                            system: "http://snomed.info/sct",
                            code: _referralDbModel.ReferrerSourceType!,
                            display: "General Dental Practice")
                    }
                ]
            }
        };
    }

    private Bundle.EntryComponent GeneratePatient()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_patientId}",
            Resource = new Patient
            {
                Meta = new Meta { Profile = ["https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Patient"] },
                Identifier =
                [
                    new Identifier
                    {
                        System = "https://fhir.nhs.uk/Id/nhs-number",
                        Value = _referralDbModel!.NhsNumber,
                        Extension =
                        [
                            new Extension
                            {
                                Url = "https://fhir.hl7.org.uk/StructureDefinition/Extension-UKCore-NHSNumberVerificationStatus",
                                Value = new CodeableConcept(
                                    system: "https://fhir.hl7.org.uk/CodeSystem/UKCore-NHSNumberVerificationStatusEngland",
                                    code: "01") // Temporary hardcode for PoC
                            }
                        ]
                    },

                    new Identifier
                    {
                        System = "https://fhir.hduhb.nhs.wales/Id/pas-identifier",
                        Value = _referralDbModel.CaseNumber!
                    }
                ],
                Address = [new Address { PostalCode = _referralDbModel.PatientPostcode }],
                GeneralPractitioner =
                [
                    new ResourceReference
                    {
                        Type = "Organization",
                        Identifier = new Identifier
                        {
                            System = "https://fhir.nhs.uk/Id/ods-organization-code",
                            Value = _referralDbModel.PatientGpPracticeCode
                        }
                    },

                    new ResourceReference
                    {
                        Type = "Practitioner",
                        Identifier = new Identifier
                        {
                            System = "https://fhir.hl7.org.uk/Id/gmc-number",
                            Value = _referralDbModel.PatientGpCode
                        }
                    }
                ]
            }
        };
    }

    private Bundle.EntryComponent GeneratePractitioner_ReceivingClinician()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_practitionerReceivingClinicianId}",
            Resource = new Practitioner
            {
                Id = "ReceivingClinician",
                Meta = new Meta { Profile = ["https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Practitioner"] },
                Identifier =
                [
                    new Identifier
                    {
                        System = "https://fhir.hl7.org.uk/Id/gdc-number",
                        Value = _referralDbModel!.ReferralAssignedConsultant
                    }
                ]
            }
        };
    }

    private Bundle.EntryComponent GeneratePractitioner_RequestingPractitioner()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_practitionerRequestingPractitionerId}",
            Resource = new Practitioner
            {
                Id = "RequestingPractitioner",
                Meta = new Meta { Profile = ["https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Practitioner"] },
                Identifier =
                [
                    new Identifier
                    {
                        System = "https://fhir.hl7.org.uk/Id/gdc-number",
                        Value = _referralDbModel!.Referrer
                    }
                ]
            }
        };
    }

    private Bundle.EntryComponent GenerateOrganization_Dha()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_organizationDhaId}",
            Resource = new Organization
            {
                Id = "DhaCode",
                Meta = new Meta { Profile = ["https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Organization"] },
                Identifier =
                [
                    new Identifier
                    {
                        System = "https://fhir.nhs.uk/Id/ods-organization-code",
                        Value = _referralDbModel!.PatientHealthBoardAreaCode
                    }
                ]
            }
        };
    }

    private Bundle.EntryComponent GenerateOrganization_ReferringPractice()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_organizationReferringPracticeId}",
            Resource = new Organization
            {
                Id = "ReferringPractice",
                Meta = new Meta { Profile = ["https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Organization"] },
                Identifier =
                [
                    new Identifier
                    {
                        System = "https://fhir.nhs.uk/Id/ods-organization-code",
                        Value = _referralDbModel!.ReferrerAddress
                    }
                ]
            }
        };
    }

    private Bundle.EntryComponent GenerateOrganization_Destination()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_organizationDestinationId}",
            Resource = new Organization
            {
                Id = "Destination",
                Meta = new Meta { Profile = ["https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Organization"] },
                Identifier =
                [
                    new Identifier
                    {
                        System = "https://fhir.nhs.uk/Id/ods-organization-code",
                        Value = "L5X6M" // Hardcode for Alpha
                    }
                ]
            }
        };
    }

    private Bundle.EntryComponent GenerateEncounter_Requester()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_encounterRequesterId}",
            Resource = new Encounter
            {
                Meta = new Meta { Profile = ["https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Encounter"] },
                Status = Encounter.EncounterStatus.Finished,
                // Values provided by Jake
                Class = new Coding(
                    system: "http://snomed.info/sct",
                    code: "327121000000104",
                    display: "Referral to dental service"),
                ServiceType = new CodeableConcept().Add(
                    system: "dhcw/SPEC",
                    code: _referralDbModel!.SpecialityIdentifier!,
                    display: "Referral to dental service"),
                Priority = new CodeableConcept(
                    system: "dhcw/lttrPriority",
                    code: _referralDbModel.LetterPriority!),
                Appointment = [new ResourceReference($"urn:uuid:{_appointmentRequesterId}")]
            }
        };
    }

    private Bundle.EntryComponent GenerateAppointment_Requester()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_appointmentRequesterId}",
            Resource = new Appointment
            {
                Meta = new Meta { Profile = ["https://fhir.hl7.org.uk/StructureDefinition/UKCore-Appointment"] },
                Extension =
                [
                    new Extension
                    {
                        Url = "BookingOrganization",
                        Value = new ResourceReference($"urn:uuid:{_organizationReferringPracticeId}")
                    }
                ],
                Status = Appointment.AppointmentStatus.Fulfilled,
                Created = _referralDbModel!.BookingDate,
                Participant =
                [
                    new Appointment.ParticipantComponent
                    {
                        Actor = new ResourceReference($"urn:uuid:{_patientId}"),
                        Status = ParticipationStatus.Accepted
                    }
                ]
            }
        };
    }

    private Bundle.EntryComponent GenerateCarePlan()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_carePlanId}",
            Resource = new CarePlan
            {
                Meta = new Meta { Profile = ["https://fhir.hl7.org.uk/StructureDefinition/UKCore-CarePlan"] },
                Status = RequestStatus.Completed,
                Intent = CarePlan.CarePlanIntent.Plan,
                Subject = new ResourceReference($"urn:uuid:{_patientId}"),
                Encounter = new ResourceReference($"urn:uuid:{_encounterRequesterId}")
            }
        };
    }

    private Bundle.EntryComponent GenerateEncounter_Planned()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_encounterPlannedId}",
            Resource = new Encounter
            {
                Meta = new Meta { Profile = ["https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Encounter"] },
                Status = Encounter.EncounterStatus.Planned,

                // Values provided by Jake
                Class = new Coding(
                    system: "http://snomed.info/sct",
                    code: "373864002",
                    display: "outpatient"),
                ServiceType = new CodeableConcept().Add(
                    system: "dhcw/SPEC",
                    code: _referralDbModel!.SpecialityIdentifier!,
                    display: "Referral to dental service"),
                Priority = new CodeableConcept(
                    system: "dhcw/lttrPriority",
                    code: _referralDbModel.LetterPriority!),
                Subject = new ResourceReference($"urn:uuid:{_patientId}"),
                Appointment = [new ResourceReference($"urn:uuid:{_appointmentPlannedId}")]
            }
        };
    }

    private Bundle.EntryComponent GenerateAppointment_Planned()
    {
        return new Bundle.EntryComponent
        {
            FullUrl = $"urn:uuid:{_appointmentPlannedId}",
            Resource = new Appointment
            {
                Meta = new Meta { Profile = ["https://fhir.hl7.org.uk/StructureDefinition/UKCore-Appointment"] },
                Status = Appointment.AppointmentStatus.Waitlist,
                Created = _currentDate.ToString("O"),
                Participant =
                [
                    new Appointment.ParticipantComponent
                    {
                        Actor = new ResourceReference($"urn:uuid:{_patientId}"),
                        Status = ParticipationStatus.Accepted
                    },

                    new Appointment.ParticipantComponent
                    {
                        Actor = new ResourceReference($"urn:uuid:{_practitionerReceivingClinicianId}"),
                        Status = ParticipationStatus.Accepted
                    }
                ]
            }
        };
    }
}
