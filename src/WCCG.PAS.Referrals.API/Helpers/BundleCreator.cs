using System.Globalization;
using Hl7.Fhir.Model;
using Hl7.Fhir.Serialization;
using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.API.Configuration;
using WCCG.PAS.Referrals.API.Constants;
using WCCG.PAS.Referrals.API.DbModels;

// ReSharper disable ArrangeObjectCreationWhenTypeEvident
// ReSharper disable UseCollectionExpression

namespace WCCG.PAS.Referrals.API.Helpers;

public class BundleCreator : IBundleCreator
{
    private readonly Dictionary<char, RequestPriority> _requestPriorityDictionary = new()
    {
        { 'U', RequestPriority.Urgent },
        { 'A', RequestPriority.Asap },
        { 'R', RequestPriority.Routine },
        { 'S', RequestPriority.Stat },
    };

    private readonly Dictionary<char, Timing.UnitsOfTime> _frequencyDictionary = new()
    {
        { 'A', Timing.UnitsOfTime.A },
        { 'D', Timing.UnitsOfTime.D },
        { 'H', Timing.UnitsOfTime.H },
        { 'M', Timing.UnitsOfTime.Mo },
        { 'S', Timing.UnitsOfTime.S },
        { 'W', Timing.UnitsOfTime.Wk },
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
            Timestamp = _currentDate,
            Type = Bundle.BundleType.Message,
            Entry = new List<Bundle.EntryComponent>
            {
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
            }
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
                    System = FhirConstants.EvenBarSystem,
                    Code = "servicerequest-request"
                },
                Destination = new List<MessageHeader.MessageDestinationComponent>
                {
                    new MessageHeader.MessageDestinationComponent
                    {
                        Receiver = new ResourceReference($"urn:uuid:{_organizationDestinationId}"),
                        Endpoint =
                            $"{_bundleCreationConfig.EReferralsBaseUrl}{_bundleCreationConfig.EReferralsCreateReferralEndpoint}|{FhirConstants.ServiceId}"
                    }
                },
                Sender = new ResourceReference($"urn:uuid:{_organizationReferringPracticeId}"),
                Source = new MessageHeader.MessageSourceComponent { Endpoint = _bundleCreationConfig.DentalUiBaseUrl },
                Reason = new CodeableConcept(
                    system: FhirConstants.ReasonBarSystem,
                    code: "new"
                ),
                Focus = new List<ResourceReference> { new ResourceReference($"urn:uuid:{_serviceRequestId}") },
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
                Meta = new Meta
                {
                    Profile =
                        new List<string> { "https://fhir.nhs.uk/StructureDefinition/BARSServiceRequest-request-referral" }
                },
                Identifier = new List<Identifier>
                {
                    new Identifier
                    {
                        System = FhirConstants.ReferralIdSystem,
                        Value = _referralDbModel!.ReferralId
                    }
                },
                Category = new List<CodeableConcept>
                {
                    new CodeableConcept().Add(
                        system: FhirConstants.ServiceRequestCategorySystem,
                        code: "referral",
                        display: "Transfer of Care")
                },
                BasedOn = new List<ResourceReference> { new ResourceReference($"urn:uuid:{_carePlanId}") },
                Status = RequestStatus.Active,
                Intent = RequestIntent.Plan,
                Priority = _requestPriorityDictionary[char.ToUpperInvariant(_referralDbModel.Priority![0])],
                Subject = new ResourceReference($"urn:uuid:{_patientId}"),
                AuthoredOn = PrimitiveTypeConverter.ConvertTo<string>(_referralDbModel.CreationDate!.Value),
                Requester = new ResourceReference($"urn:uuid:{_practitionerRequestingPractitionerId}"),
                Performer = new List<ResourceReference> { new ResourceReference($"urn:uuid:{_organizationDhaId}") },
                OrderDetail =
                    new List<CodeableConcept>
                    {
                        new CodeableConcept(system: FhirConstants.WlistCodesSystem, code: _referralDbModel.WaitingList!),
                        new CodeableConcept(system: FhirConstants.IntentReferValuesSystem, code: _referralDbModel.IntendedManagement!),
                        new CodeableConcept(system: FhirConstants.PatientCategorySystem, code: _referralDbModel.PatientCategory!),
                        new CodeableConcept(system: FhirConstants.DatonsysSystem,
                            code: PrimitiveTypeConverter.ConvertTo<string>(_referralDbModel.HealthBoardReceiveDate!.Value)),
                        new CodeableConcept(system: FhirConstants.RiskFactorSystem, code: _referralDbModel.HealthRiskFactor!)
                    },
                Encounter = new ResourceReference($"urn:uuid:{_encounterRequesterId}"),
                Occurrence = new Timing
                {
                    Event = new List<string> { PrimitiveTypeConverter.ConvertTo<string>(_referralDbModel.FirstAppointmentDate!.Value) },
                    Repeat = new Timing.RepeatComponent
                    {
                        Period = decimal.Parse(_referralDbModel.RepeatPeriod.AsSpan()[..^1], CultureInfo.InvariantCulture),
                        PeriodUnit = _frequencyDictionary[char.ToUpperInvariant(_referralDbModel.RepeatPeriod!.AsSpan()[^1])]
                    }
                },
                LocationCode =
                    new List<CodeableConcept>
                    {
                        new CodeableConcept(
                            system: FhirConstants.OdcOrganizationCodeSystem,
                            code: _referralDbModel.ReferralAssignedLocation!)
                    },
                Extension =
                    new List<Extension>
                    {
                        new Extension
                        {
                            Url = "https://fhir.hl7.org.uk/StructureDefinition/Extension-UKCore-SourceOfServiceRequest",
                            Value = new CodeableConcept().Add(
                                system: FhirConstants.SctSystem,
                                code: _referralDbModel.ReferrerSourceType!,
                                display: "General Dental Practice")
                        }
                    }
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
                Meta = new Meta { Profile = new List<string> { "https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Patient" } },
                Identifier =
                    new List<Identifier>
                    {
                        new Identifier
                        {
                            System = FhirConstants.NhsNumberSystem,
                            Value = _referralDbModel!.NhsNumber,
                            Extension =
                                new List<Extension>
                                {
                                    new Extension
                                    {
                                        Url = "https://fhir.hl7.org.uk/StructureDefinition/Extension-UKCore-NHSNumberVerificationStatus",
                                        Value = new CodeableConcept(
                                            system: FhirConstants.VerificationStatusSystem,
                                            code: "01") // Temporary hardcode for PoC
                                    }
                                }
                        },
                        new Identifier
                        {
                            System = FhirConstants.PasIdentifierSystem,
                            Value = _referralDbModel.CaseNumber!
                        }
                    },
                Address = new List<Address> { new Address { PostalCode = _referralDbModel.PatientPostcode } },
                GeneralPractitioner =
                    new List<ResourceReference>
                    {
                        new ResourceReference
                        {
                            Type = "Organization",
                            Identifier = new Identifier
                            {
                                System = FhirConstants.OdcOrganizationCodeSystem,
                                Value = _referralDbModel.PatientGpPracticeCode
                            }
                        },
                        new ResourceReference
                        {
                            Type = "Practitioner",
                            Identifier = new Identifier
                            {
                                System = FhirConstants.GmcNumberSystem,
                                Value = _referralDbModel.PatientGpCode
                            }
                        }
                    }
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
                Id = FhirConstants.ReceivingClinicianId,
                Meta = new Meta
                {
                    Profile =
                        new List<string> { "https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Practitioner" }
                },
                Identifier =
                    new List<Identifier>
                    {
                        new Identifier
                        {
                            System = FhirConstants.GdcNumberSystem,
                            Value = _referralDbModel!.ReferralAssignedConsultant
                        }
                    }
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
                Id = FhirConstants.RequestingPractitionerId,
                Meta = new Meta
                {
                    Profile =
                        new List<string> { "https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Practitioner" }
                },
                Identifier =
                    new List<Identifier>
                    {
                        new Identifier
                        {
                            System = FhirConstants.GdcNumberSystem,
                            Value = _referralDbModel!.Referrer
                        }
                    }
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
                Id = FhirConstants.DhaCodeId,
                Meta = new Meta
                {
                    Profile =
                        new List<string> { "https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Organization" }
                },
                Identifier =
                    new List<Identifier>
                    {
                        new Identifier
                        {
                            System = FhirConstants.OdcOrganizationCodeSystem,
                            Value = _referralDbModel!.PatientHealthBoardAreaCode
                        }
                    }
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
                Id = FhirConstants.ReferringPracticeId,
                Meta = new Meta
                {
                    Profile =
                        new List<string> { "https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Organization" }
                },
                Identifier =
                    new List<Identifier>
                    {
                        new Identifier
                        {
                            System = FhirConstants.OdcOrganizationCodeSystem,
                            Value = _referralDbModel!.ReferrerAddress
                        }
                    }
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
                Id = FhirConstants.DestinationId,
                Meta = new Meta
                {
                    Profile =
                        new List<string> { "https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Organization" }
                },
                Identifier =
                    new List<Identifier>
                    {
                        new Identifier
                        {
                            System = FhirConstants.OdcOrganizationCodeSystem,
                            Value = "L5X6M" // Hardcode for Alpha
                        }
                    }
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
                Meta = new Meta
                {
                    Profile = new List<string> { "https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Encounter" }
                },
                Status = Encounter.EncounterStatus.Finished,
                // Values provided by Jake
                Class = new Coding(
                    system: FhirConstants.SctSystem,
                    code: "327121000000104",
                    display: "Referral to dental service"),
                ServiceType = new CodeableConcept().Add(
                    system: FhirConstants.SpecialityIdentifierSystem,
                    code: _referralDbModel!.SpecialityIdentifier!,
                    display: "Referral to dental service"),
                Priority = new CodeableConcept(
                    system: FhirConstants.LetterPrioritySystem,
                    code: _referralDbModel.LetterPriority!),
                Appointment = new List<ResourceReference> { new ResourceReference($"urn:uuid:{_appointmentRequesterId}") }
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
                Meta = new Meta
                {
                    Profile =
                        new List<string> { "https://fhir.hl7.org.uk/StructureDefinition/UKCore-Appointment" }
                },
                Extension =
                    new List<Extension>
                    {
                        new Extension
                        {
                            Url = FhirConstants.BookingOrganisation,
                            Value = new ResourceReference($"urn:uuid:{_organizationReferringPracticeId}")
                        }
                    },
                Status = Appointment.AppointmentStatus.Fulfilled,
                Created = PrimitiveTypeConverter.ConvertTo<string>(_referralDbModel!.BookingDate!.Value),
                Participant =
                    new List<Appointment.ParticipantComponent>
                    {
                        new Appointment.ParticipantComponent
                        {
                            Actor = new ResourceReference($"urn:uuid:{_patientId}"),
                            Status = ParticipationStatus.Accepted
                        }
                    }
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
                Meta =
                    new Meta { Profile = new List<string> { "https://fhir.hl7.org.uk/StructureDefinition/UKCore-CarePlan" } },
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
                Meta = new Meta
                {
                    Profile = new List<string> { "https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Encounter" }
                },
                Status = Encounter.EncounterStatus.Planned,

                // Values provided by Jake
                Class = new Coding(
                    system: FhirConstants.SctSystem,
                    code: "373864002",
                    display: "outpatient"),
                ServiceType = new CodeableConcept().Add(
                    system: FhirConstants.SpecialityIdentifierSystem,
                    code: _referralDbModel!.SpecialityIdentifier!,
                    display: "Referral to dental service"),
                Priority = new CodeableConcept(
                    system: FhirConstants.LetterPrioritySystem,
                    code: _referralDbModel.LetterPriority!),
                Subject = new ResourceReference($"urn:uuid:{_patientId}"),
                Appointment = new List<ResourceReference> { new ResourceReference($"urn:uuid:{_appointmentPlannedId}") }
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
                Meta = new Meta
                {
                    Profile =
                        new List<string> { "https://fhir.hl7.org.uk/StructureDefinition/UKCore-Appointment" }
                },
                Status = Appointment.AppointmentStatus.Waitlist,
                Created = PrimitiveTypeConverter.ConvertTo<string>(_currentDate),
                Participant =
                    new List<Appointment.ParticipantComponent>
                    {
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
                    }
            }
        };
    }
}
