using System.Globalization;
using AutoFixture;
using FluentAssertions;
using Hl7.Fhir.Model;
using Microsoft.Extensions.Options;
using WCCG.PAS.Referrals.API.Configuration;
using WCCG.PAS.Referrals.API.Constants;
using WCCG.PAS.Referrals.API.DbModels;
using WCCG.PAS.Referrals.API.Helpers;
using WCCG.PAS.Referrals.API.Unit.Tests.Extensions;

namespace WCCG.PAS.Referrals.API.Unit.Tests.Helpers;

public class BundleCreatorTests
{
    private readonly BundleCreator _sut;
    private readonly BundleCreationConfig _config;
    private readonly IFixture _fixture = new Fixture().WithCustomizations();

    public BundleCreatorTests()
    {
        _config = _fixture.Create<BundleCreationConfig>();
        _fixture.Mock<IOptions<BundleCreationConfig>>().SetupGet(x => x.Value).Returns(_config);

        _sut = _fixture.CreateWithFrozen<BundleCreator>();
    }

    [Fact]
    public void CreateBundleShouldCreateMessageHeader()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var destinationOrganization = GetEntryComponentByResource<Organization>(bundle, FhirConstants.DestinationId);
        var referringPracticeOrganization = GetEntryComponentByResource<Organization>(bundle, FhirConstants.ReferringPracticeId);
        var serviceRequest = GetEntryComponentByResource<ServiceRequest>(bundle);

        var messageHeaderEntry = GetEntryComponentByResource<MessageHeader>(bundle);
        messageHeaderEntry.Should().NotBeNull();
        messageHeaderEntry!.FullUrl.Should().BeValidFhirUrl();

        var messageHeader = messageHeaderEntry.Resource as MessageHeader;
        (messageHeader!.Event as Coding)!.System.Should().Be(FhirConstants.EvenBarSystem);
        (messageHeader.Event as Coding)!.Code.Should().Be("servicerequest-request");
        messageHeader.Destination.Should().HaveCount(1);
        messageHeader.Destination[0].Receiver.Reference.Should().Be(destinationOrganization!.FullUrl);
        messageHeader.Destination[0].Endpoint.Should()
            .Be($"{_config.EReferralsBaseUrl}{_config.EReferralsCreateReferralEndpoint}|0000000000");
        messageHeader.Sender.Reference.Should().Be(referringPracticeOrganization!.FullUrl);
        messageHeader.Source.Endpoint.Should().Be(_config.DentalUiBaseUrl);
        messageHeader.Reason.Coding.Should().HaveCount(1);
        messageHeader.Reason.Coding[0].System.Should().Be(FhirConstants.ReasonBarSystem);
        messageHeader.Reason.Coding[0].Code.Should().Be("new");
        messageHeader.Focus.Should().HaveCount(1);
        messageHeader.Focus[0].Reference.Should().Be(serviceRequest!.FullUrl);
        messageHeader.Definition.Should().Be("https://fhir.nhs.uk/MessageDefinition/bars-message-servicerequest-request-referral");
    }

    [Fact]
    public void CreateBundleShouldCreateServiceRequest()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();
        DateTimeOffset datePlaceholder;

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var carePlan = GetEntryComponentByResource<CarePlan>(bundle);
        var patient = GetEntryComponentByResource<Patient>(bundle);
        var referringPractitioner = GetEntryComponentByResource<Practitioner>(bundle, FhirConstants.RequestingPractitionerId);
        var dhaOrganization = GetEntryComponentByResource<Organization>(bundle, FhirConstants.DhaCodeId);
        var requesterEncounter = GetEncounterByStatus(bundle, Encounter.EncounterStatus.Finished);

        var serviceRequestEntry = GetEntryComponentByResource<ServiceRequest>(bundle);
        serviceRequestEntry.Should().NotBeNull();
        serviceRequestEntry!.FullUrl.Should().BeValidFhirUrl();

        var serviceRequest = serviceRequestEntry.Resource as ServiceRequest;
        serviceRequest!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.nhs.uk/StructureDefinition/BARSServiceRequest-request-referral");
        serviceRequest.Identifier.Should().HaveCount(2);
        serviceRequest.Identifier[0].System.Should().Be(FhirConstants.LinkIdSystem);
        serviceRequest.Identifier[0].Value.Should().StartWith("REF")
            .And.Subject.Split("REF")[1].Should().Match(x =>
                DateTimeOffset.TryParseExact(x, "yyyyMMddHHmm", CultureInfo.InvariantCulture, DateTimeStyles.None, out datePlaceholder));
        serviceRequest.Identifier[1].System.Should().Be(FhirConstants.ReferralIdSystem);
        serviceRequest.Identifier[1].Value.Should().Be(dbModel.ReferralId);
        serviceRequest.Category.Should().HaveCount(1);
        serviceRequest.Category[0].Coding.Should().HaveCount(1);
        serviceRequest.Category[0].Coding[0].System.Should().Be(FhirConstants.ServiceRequestCategorySystem);
        serviceRequest.Category[0].Coding[0].Code.Should().Be("referral");
        serviceRequest.Category[0].Coding[0].Display.Should().Be("Transfer of Care");
        serviceRequest.BasedOn.Should().HaveCount(1);
        serviceRequest.BasedOn[0].Reference.Should().Be(carePlan!.FullUrl);
        serviceRequest.Status.Should().Be(RequestStatus.Active);
        serviceRequest.Intent.Should().Be(RequestIntent.Plan);
        serviceRequest.Priority.Should().Be(RequestPriority.Routine);
        serviceRequest.Subject.Reference.Should().Be(patient!.FullUrl);
        serviceRequest.AuthoredOn.Should().Be(dbModel.CreationDate);
        serviceRequest.Requester.Reference.Should().Be(referringPractitioner!.FullUrl);
        serviceRequest.Performer.Should().HaveCount(1);
        serviceRequest.Performer[0].Reference.Should().Be(dhaOrganization!.FullUrl);
        serviceRequest.OrderDetail.Should().HaveCount(5);
        serviceRequest.OrderDetail[0].Coding.Should().HaveCount(1);
        serviceRequest.OrderDetail[0].Coding[0].System.Should().Be(FhirConstants.WlistCodesSystem);
        serviceRequest.OrderDetail[0].Coding[0].Code.Should().Be(dbModel.WaitingList);
        serviceRequest.OrderDetail[1].Coding.Should().HaveCount(1);
        serviceRequest.OrderDetail[1].Coding[0].System.Should().Be(FhirConstants.IntentReferValuesSystem);
        serviceRequest.OrderDetail[1].Coding[0].Code.Should().Be(dbModel.IntendedManagement);
        serviceRequest.OrderDetail[2].Coding.Should().HaveCount(1);
        serviceRequest.OrderDetail[2].Coding[0].System.Should().Be(FhirConstants.PatientCategorySystem);
        serviceRequest.OrderDetail[2].Coding[0].Code.Should().Be(dbModel.PatientCategory);
        serviceRequest.OrderDetail[3].Coding.Should().HaveCount(1);
        serviceRequest.OrderDetail[3].Coding[0].System.Should().Be(FhirConstants.DatonsysSystem);
        serviceRequest.OrderDetail[3].Coding[0].Code.Should().Be(dbModel.HealthBoardReceiveDate);
        serviceRequest.OrderDetail[4].Coding.Should().HaveCount(1);
        serviceRequest.OrderDetail[4].Coding[0].System.Should().Be(FhirConstants.RiskFactorSystem);
        serviceRequest.OrderDetail[4].Coding[0].Code.Should().Be(dbModel.HealthRiskFactor);
        serviceRequest.Encounter.Reference.Should().Be(requesterEncounter!.FullUrl);
        (serviceRequest.Occurrence as Timing)!.Event.Should().HaveCount(1);
        (serviceRequest.Occurrence as Timing)!.Event.First().Should().Be(dbModel.FirstAppointmentDate);
        (serviceRequest.Occurrence as Timing)!.Repeat.Period.Should().Be(6);
        (serviceRequest.Occurrence as Timing)!.Repeat.PeriodUnit.Should().Be(Timing.UnitsOfTime.D);
        serviceRequest.LocationCode.Should().HaveCount(1);
        serviceRequest.LocationCode[0].Coding.Should().HaveCount(1);
        serviceRequest.LocationCode[0].Coding[0].System.Should().Be(FhirConstants.OdcOrganizationCodeSystem);
        serviceRequest.LocationCode[0].Coding[0].Code.Should().Be(dbModel.ReferralAssignedLocation);
        serviceRequest.Extension.Should().HaveCount(1);
        serviceRequest.Extension[0].Url.Should().Be("https://fhir.hl7.org.uk/StructureDefinition/Extension-UKCore-SourceOfServiceRequest");
        (serviceRequest.Extension[0].Value as CodeableConcept)!.Coding.Should().HaveCount(1);
        (serviceRequest.Extension[0].Value as CodeableConcept)!.Coding[0].System.Should().Be(FhirConstants.SctSystem);
        (serviceRequest.Extension[0].Value as CodeableConcept)!.Coding[0].Code.Should().Be(dbModel.ReferrerSourceType);
        (serviceRequest.Extension[0].Value as CodeableConcept)!.Coding[0].Display.Should().Be("General Dental Practice");
    }

    [Fact]
    public void CreateBundleShouldCreatePatient()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var patientEntry = GetEntryComponentByResource<Patient>(bundle);
        patientEntry.Should().NotBeNull();
        patientEntry!.FullUrl.Should().BeValidFhirUrl();

        var patient = patientEntry.Resource as Patient;
        patient!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Patient");
        patient.Identifier.Should().HaveCount(2);
        patient.Identifier[0].System.Should().Be(FhirConstants.NhsNumberSystem);
        patient.Identifier[0].Value.Should().Be(dbModel.NhsNumber);
        patient.Identifier[0].Extension.Should().HaveCount(1);
        patient.Identifier[0].Extension[0].Url.Should()
            .Be("https://fhir.hl7.org.uk/StructureDefinition/Extension-UKCore-NHSNumberVerificationStatus");
        (patient.Identifier[0].Extension[0].Value as CodeableConcept)!.Coding.Should().HaveCount(1);
        (patient.Identifier[0].Extension[0].Value as CodeableConcept)!.Coding[0].System.Should()
            .Be(FhirConstants.VerificationStatusSystem);
        (patient.Identifier[0].Extension[0].Value as CodeableConcept)!.Coding[0].Code.Should().Be("01");
        patient.Identifier[1].System.Should().Be(FhirConstants.PasIdentifierSystem);
        patient.Identifier[1].Value.Should().Be(dbModel.CaseNumber);
        patient.Address.Should().HaveCount(1);
        patient.Address[0].PostalCode.Should().Be(dbModel.PatientPostcode);
        patient.GeneralPractitioner.Should().HaveCount(2);
        patient.GeneralPractitioner[0].Type.Should().Be(FhirConstants.OrganizationType);
        patient.GeneralPractitioner[0].Identifier.System.Should().Be(FhirConstants.OdcOrganizationCodeSystem);
        patient.GeneralPractitioner[0].Identifier.Value.Should().Be(dbModel.PatientGpPracticeCode);
        patient.GeneralPractitioner[1].Type.Should().Be(FhirConstants.PractitionerType);
        patient.GeneralPractitioner[1].Identifier.System.Should().Be(FhirConstants.GmcNumberSystem);
        patient.GeneralPractitioner[1].Identifier.Value.Should().Be(dbModel.PatientGpCode);
    }

    [Fact]
    public void CreateBundleShouldCreateReceivingClinicianPractitioner()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var practitionerEntry = GetEntryComponentByResource<Practitioner>(bundle, FhirConstants.ReceivingClinicianId);
        practitionerEntry.Should().NotBeNull();
        practitionerEntry!.FullUrl.Should().BeValidFhirUrl();

        var practitioner = practitionerEntry.Resource as Practitioner;
        practitioner!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Practitioner");
        practitioner.Identifier.Should().HaveCount(1);
        practitioner.Identifier[0].System.Should().Be(FhirConstants.GdcNumberSystem);
        practitioner.Identifier[0].Value.Should().Be(dbModel.ReferralAssignedConsultant);
    }

    [Fact]
    public void CreateBundleShouldCreateRequestingPractitioner()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var practitionerEntry = GetEntryComponentByResource<Practitioner>(bundle, FhirConstants.RequestingPractitionerId);
        practitionerEntry.Should().NotBeNull();
        practitionerEntry!.FullUrl.Should().BeValidFhirUrl();

        var practitioner = practitionerEntry.Resource as Practitioner;
        practitioner!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Practitioner");
        practitioner.Identifier.Should().HaveCount(1);
        practitioner.Identifier[0].System.Should().Be(FhirConstants.GdcNumberSystem);
        practitioner.Identifier[0].Value.Should().Be(dbModel.Referrer);
    }

    [Fact]
    public void CreateBundleShouldCreateDhaOrganization()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var organizationEntry = GetEntryComponentByResource<Organization>(bundle, FhirConstants.DhaCodeId);
        organizationEntry.Should().NotBeNull();
        organizationEntry!.FullUrl.Should().BeValidFhirUrl();

        var organization = organizationEntry.Resource as Organization;
        organization!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Organization");
        organization.Identifier.Should().HaveCount(1);
        organization.Identifier[0].System.Should().Be(FhirConstants.OdcOrganizationCodeSystem);
        organization.Identifier[0].Value.Should().Be(dbModel.PatientHealthBoardAreaCode);
    }

    [Fact]
    public void CreateBundleShouldCreateReferringPracticeOrganization()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var organizationEntry = GetEntryComponentByResource<Organization>(bundle, FhirConstants.ReferringPracticeId);
        organizationEntry.Should().NotBeNull();
        organizationEntry!.FullUrl.Should().BeValidFhirUrl();

        var organization = organizationEntry.Resource as Organization;
        organization!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Organization");
        organization.Identifier.Should().HaveCount(1);
        organization.Identifier[0].System.Should().Be(FhirConstants.OdcOrganizationCodeSystem);
        organization.Identifier[0].Value.Should().Be(dbModel.ReferrerAddress);
    }

    [Fact]
    public void CreateBundleShouldCreateDestinationOrganization()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var organizationEntry = GetEntryComponentByResource<Organization>(bundle, FhirConstants.DestinationId);
        organizationEntry.Should().NotBeNull();
        organizationEntry!.FullUrl.Should().BeValidFhirUrl();

        var organization = organizationEntry.Resource as Organization;
        organization!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Organization");
        organization.Identifier.Should().HaveCount(1);
        organization.Identifier[0].System.Should().Be(FhirConstants.OdcOrganizationCodeSystem);
        organization.Identifier[0].Value.Should().Be("L5X6M");
    }

    [Fact]
    public void CreateBundleShouldCreateRequesterEncounter()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var appointment = GetAppointmentByStatus(bundle, Appointment.AppointmentStatus.Fulfilled);

        var encounterEntry = GetEncounterByStatus(bundle, Encounter.EncounterStatus.Finished);
        encounterEntry.Should().NotBeNull();
        encounterEntry!.FullUrl.Should().BeValidFhirUrl();

        var encounter = encounterEntry.Resource as Encounter;
        encounter!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Encounter");
        encounter.Class.System.Should().Be(FhirConstants.SctSystem);
        encounter.Class.Code.Should().Be("327121000000104");
        encounter.Class.Display.Should().Be("Referral to dental service");
        encounter.ServiceType.Coding.Should().HaveCount(1);
        encounter.ServiceType.Coding[0].System.Should().Be(FhirConstants.SpecialityIdentifierSystem);
        encounter.ServiceType.Coding[0].Code.Should().Be(dbModel.SpecialityIdentifier);
        encounter.ServiceType.Coding[0].Display.Should().Be("Referral to dental service");
        encounter.Priority.Coding.Should().HaveCount(1);
        encounter.Priority.Coding[0].System.Should().Be(FhirConstants.LetterPrioritySystem);
        encounter.Priority.Coding[0].Code.Should().Be(dbModel.LetterPriority);
        encounter.Appointment.Should().HaveCount(1);
        encounter.Appointment[0].Reference.Should().Be(appointment!.FullUrl);
    }

    [Fact]
    public void CreateBundleShouldCreateRequesterAppointment()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var organization = GetEntryComponentByResource<Organization>(bundle, FhirConstants.ReferringPracticeId);
        var patient = GetEntryComponentByResource<Patient>(bundle);

        var appointmentEntry = GetAppointmentByStatus(bundle, Appointment.AppointmentStatus.Fulfilled);
        appointmentEntry.Should().NotBeNull();
        appointmentEntry!.FullUrl.Should().BeValidFhirUrl();

        var appointment = appointmentEntry.Resource as Appointment;
        appointment!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.hl7.org.uk/StructureDefinition/UKCore-Appointment");
        appointment.Extension.Should().HaveCount(1);
        appointment.Extension[0].Url.Should().Be(FhirConstants.BookingOrganisation);
        (appointment.Extension[0].Value as ResourceReference)!.Reference.Should().Be(organization!.FullUrl);
        appointment.Created.Should().Be(dbModel.BookingDate);
        appointment.Participant.Should().HaveCount(1);
        appointment.Participant[0].Actor.Reference.Should().Be(patient!.FullUrl);
        appointment.Participant[0].Status.Should().Be(ParticipationStatus.Accepted);
    }

    [Fact]
    public void CreateBundleShouldCreateCarePlan()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var patient = GetEntryComponentByResource<Patient>(bundle);
        var encounter = GetEncounterByStatus(bundle, Encounter.EncounterStatus.Finished);

        var carePlanEntry = GetEntryComponentByResource<CarePlan>(bundle);
        carePlanEntry.Should().NotBeNull();
        carePlanEntry!.FullUrl.Should().BeValidFhirUrl();

        var carePlan = carePlanEntry.Resource as CarePlan;
        carePlan!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.hl7.org.uk/StructureDefinition/UKCore-CarePlan");
        carePlan.Status.Should().Be(RequestStatus.Completed);
        carePlan.Intent.Should().Be(CarePlan.CarePlanIntent.Plan);
        carePlan.Subject.Reference.Should().Be(patient!.FullUrl);
        carePlan.Encounter.Reference.Should().Be(encounter!.FullUrl);
    }

    [Fact]
    public void CreateBundleShouldCreatePlannedEncounter()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var appointment = GetAppointmentByStatus(bundle, Appointment.AppointmentStatus.Waitlist);
        var patient = GetEntryComponentByResource<Patient>(bundle);

        var encounterEntry = GetEncounterByStatus(bundle, Encounter.EncounterStatus.Planned);
        encounterEntry.Should().NotBeNull();
        encounterEntry!.FullUrl.Should().BeValidFhirUrl();

        var encounter = encounterEntry.Resource as Encounter;
        encounter!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.nhs.wales/StructureDefinition/DataStandardsWales-Encounter");
        encounter.Class.System.Should().Be(FhirConstants.SctSystem);
        encounter.Class.Code.Should().Be("373864002");
        encounter.Class.Display.Should().Be("outpatient");
        encounter.ServiceType.Coding.Should().HaveCount(1);
        encounter.ServiceType.Coding[0].System.Should().Be(FhirConstants.SpecialityIdentifierSystem);
        encounter.ServiceType.Coding[0].Code.Should().Be(dbModel.SpecialityIdentifier);
        encounter.ServiceType.Coding[0].Display.Should().Be("Referral to dental service");
        encounter.Priority.Coding.Should().HaveCount(1);
        encounter.Priority.Coding[0].System.Should().Be(FhirConstants.LetterPrioritySystem);
        encounter.Priority.Coding[0].Code.Should().Be(dbModel.LetterPriority);
        encounter.Subject.Reference.Should().Be(patient!.FullUrl);
        encounter.Appointment.Should().HaveCount(1);
        encounter.Appointment[0].Reference.Should().Be(appointment!.FullUrl);
    }

    [Fact]
    public void CreateBundleShouldCreatePlannedAppointment()
    {
        //Arrange
        var dbModel = CreateValidReferralDbModel();
        DateTimeOffset datePlaceholder;

        //Act
        var bundle = _sut.CreateBundle(dbModel);

        //Assert
        var practitioner = GetEntryComponentByResource<Practitioner>(bundle, FhirConstants.ReceivingClinicianId);
        var patient = GetEntryComponentByResource<Patient>(bundle);

        var appointmentEntry = GetAppointmentByStatus(bundle, Appointment.AppointmentStatus.Waitlist);
        appointmentEntry.Should().NotBeNull();
        appointmentEntry!.FullUrl.Should().BeValidFhirUrl();

        var appointment = appointmentEntry.Resource as Appointment;
        appointment!.Meta.Profile.Should().HaveCount(1)
            .And.Contain("https://fhir.hl7.org.uk/StructureDefinition/UKCore-Appointment");
        appointment.Created.Should().Match(x =>
            DateTimeOffset.TryParseExact(x, "O", CultureInfo.InvariantCulture, DateTimeStyles.None, out datePlaceholder));
        appointment.Participant.Should().HaveCount(2);
        appointment.Participant[0].Actor.Reference.Should().Be(patient!.FullUrl);
        appointment.Participant[0].Status.Should().Be(ParticipationStatus.Accepted);
        appointment.Participant[1].Actor.Reference.Should().Be(practitioner!.FullUrl);
        appointment.Participant[1].Status.Should().Be(ParticipationStatus.Accepted);
    }

    private static Bundle.EntryComponent? GetEntryComponentByResource<T>(Bundle bundle, string? id = null)
    {
        var resultList = bundle.Children.OfType<Bundle.EntryComponent>()
            .Where(x => x.Resource is not null
                        && x.Resource.TypeName.Equals(typeof(T).Name, StringComparison.OrdinalIgnoreCase));

        return id is not null
            ? resultList.FirstOrDefault(x => x.Resource.Id.Equals(id, StringComparison.OrdinalIgnoreCase))
            : resultList.FirstOrDefault();
    }

    private static Bundle.EntryComponent? GetEncounterByStatus(Bundle bundle, Encounter.EncounterStatus status)
    {
        return bundle.Children.OfType<Bundle.EntryComponent>()
            .FirstOrDefault(x => x.Resource is not null
                                 && x.Resource.TypeName.Equals(nameof(Encounter), StringComparison.OrdinalIgnoreCase)
                                 && (x.Resource as Encounter)!.Status == status);
    }

    private static Bundle.EntryComponent? GetAppointmentByStatus(Bundle bundle, Appointment.AppointmentStatus status)
    {
        return bundle.Children.OfType<Bundle.EntryComponent>()
            .FirstOrDefault(x => x.Resource is not null
                                 && x.Resource.TypeName.Equals(nameof(Appointment), StringComparison.OrdinalIgnoreCase)
                                 && (x.Resource as Appointment)!.Status == status);
    }

    private ReferralDbModel CreateValidReferralDbModel()
    {
        return _fixture.Build<ReferralDbModel>()
            .With(x => x.Priority, "R")
            .With(x => x.RepeatPeriod, "6D")
            .Create();
    }
}
