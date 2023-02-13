using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Security;
using System.Text;
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;

namespace ServiceTitan
{
  public class ServiceTitan
{
    public static DateTime lastQuery;
    const int minMsSinceLastQuery = 500;
    public static bool useSandbox = false;
    const string productionEnvironment = "api.servicetitan.io";
    const string productionAuthEnvironment = "auth.servicetitan.io";
    const string sandboxEnvironment = "api-integration.servicetitan.io";
    const string sandboxAuthEnvironment = "auth-integration.servicetitan.io";



    public class Helpers {
        public static Types.CRM.CustomerResponse getCustomerById(oAuth2.AccessToken accesstoken, string appId, int tenantId, int customerId) {
            var temp = Functions.CRM.getCustomer(accesstoken, appId, tenantId, customerId);
            Types.CRM.CustomerResponse customer = null;
            if (temp is Types.CRM.CustomerResponse) {
                customer = (Types.CRM.CustomerResponse)temp;
            }
            return customer;
        }
    }
    public class oAuth2
    {
        public class Credentials
        {
            /// <summary>
            ///             ''' grant_type: Right now, should always be 'client_credentials'.
            ///             ''' </summary>
            public string grant_type = "client_credentials";
            /// <summary>
            ///             ''' client_id: Obtained from the Production Environment/Integration Environment. See https://developer.servicetitan.io/docs/get-going-manage-client-id-and-secret/
            ///             ''' </summary>
            public string client_id;
            /// <summary>
            ///             ''' client_secret: Obtained from the Production Environment/Integration Environment. See https://developer.servicetitan.io/docs/get-going-manage-client-id-and-secret/
            ///             ''' </summary>
            public string client_secret;
        }
        public class AccessToken
        {
            /// <summary>
            ///             ''' Required to make calls to ServiceTitan Endpoints.
            ///             ''' </summary>
            public string access_token;
            /// <summary>
            ///             ''' The access_token will expire in that many seconds from the original request. See the expires_at property for the exact expiration date and time.
            ///             ''' </summary>
            public int expires_in;
            public string token_type;
            public string scope;
            /// <summary>
            ///             ''' The access_token will expire on this date and time (Local system time).
            ///             ''' </summary>
            public DateTime expires_at;
        }
        public static AccessToken getAccessToken(Credentials credentials)
        {
            string domain;
            if (useSandbox == true)
                domain = "https://" + sandboxAuthEnvironment;
            else
                domain = "https://" + productionAuthEnvironment;
            string requesturi = domain + "/connect/token";
            string args = "grant_type=" + credentials.grant_type;
            args += "&client_id=" + credentials.client_id;
            args += "&client_secret=" + credentials.client_secret;

            WebRequest req = WebRequest.Create(requesturi);
            req.Method = "POST";
            req.Timeout = 999999;
            req.ContentType = "application/x-www-form-urlencoded";
            byte[] bytearray = Encoding.UTF8.GetBytes(args);
            req.ContentLength = bytearray.Length;
            Stream datastream = req.GetRequestStream();
            datastream.Write(bytearray, 0, bytearray.Length);
            datastream.Close();

            WebResponse response = req.GetResponse();
            Stream buffer = response.GetResponseStream();
            StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
            string output = streamread.ReadToEnd();
            streamread.Close();
            buffer.Close();
            response.Close();

            AccessToken accesstoken = JsonConvert.DeserializeObject<AccessToken>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            accesstoken.expires_at = DateTime.Now.AddSeconds(accesstoken.expires_in);
            return accesstoken;
        }
    }
    public class Types
    {
        public class Accounting
        {
            /// <summary>
            ///             ''' AdjustmentInvoiceCreateRequest: 'AdjustmentToId' is required.
            ///             ''' </summary>
            public class AdjustmentInvoiceCreateRequest
            {
                public string number { get; set; }
                public int typeId { get; set; }
                public string invoicedOn { get; set; }
                public int subtotal { get; set; }
                public int tax { get; set; }
                public string summary { get; set; }
                [JsonProperty("royaltyStatus")]
                public Royaltystatus royaltyStatus_ { get; set; }
                public DateTime royaltyDate { get; set; }
                public string royaltySentOn { get; set; }
                public string royaltyMemo { get; set; }
                public string exportId { get; set; }
                public InvoiceItemUpdateRequest items { get; set; } = new InvoiceItemUpdateRequest();
                public List<PaymentSettlementUpdateRequest> payments { get; set; }
                public int adjustmentToId { get; set; }
            }

            public class Royaltystatus
            {
                public string status { get; set; }
            }

            public class InvoiceItemUpdateRequest
            {
                public int skuId { get; set; }
                public string skuName { get; set; }
                public int technicianId { get; set; }
                public string description { get; set; }
                public decimal quantity { get; set; }
                public decimal unitPrice { get; set; }
                public decimal cost { get; set; }
                public bool isAddOn { get; set; }
                public string signature { get; set; }
                public string technicianAcknowledgementSignature { get; set; }
                public DateTime installedOn { get; set; }
                public string inventoryWarehouseName { get; set; }
                public bool skipUpdatingMembershipPrices { get; set; }
                public string itemGroupName { get; set; }
                public int itemGroupRootId { get; set; }
                public int id { get; set; }
            }

            public class PaymentSettlementUpdateRequest
            {
                public int id { get; set; }
                public Settlementstatus settlementStatus { get; set; }
                public DateTime settlementDate { get; set; }
            }

            public class Settlementstatus
            {
                public string status { get; set; }
            }

            public class InvoiceResponse
            {
                public int id { get; set; }
                public string syncStatus { get; set; }
                public string summary { get; set; }
                public string referenceNumber { get; set; }
                public DateTime invoiceDate { get; set; }
                public DateTime dueDate { get; set; }
                public string subTotal { get; set; }
                public decimal salesTax { get; set; }
                public SalesTaxResponse salesTaxCode { get; set; }
                public decimal total { get; set; }
                public decimal balance { get; set; }
                public NameFieldResponse customer { get; set; }
                public AddressResponse customerAddress { get; set; }
                public AddressResponse locationAddress { get; set; }
                public NameFieldResponse businessUnit { get; set; }
                public string termName { get; set; }
                public string createdBy { get; set; }
                public BatchResponse batch { get; set; }
                public DateTime modifiedOn { get; set; }
                public int adjustmentToId { get; set; }
                public JobResponse job { get; set; }
                public int projectId { get; set; }
                public RoyaltyResponse royalty { get; set; }
                public EmployeeInfoResponse employeeInfo { get; set; }
                public string commissionEligibilityDate { get; set; }
                public List<InvoiceItemResponse> items { get; set; }
                public List<CustomFieldResponse> customFields { get; set; }
            }

            public class SalesTaxResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public decimal taxRate { get; set; }
            }

            public class NameFieldResponse
            {
                public int id { get; set; }
                public string name { get; set; }
            }

            public class AddressResponse
            {
                public string street { get; set; }
                public string unit { get; set; }
                public string city { get; set; }
                public string state { get; set; }
                public string zip { get; set; }
                public string country { get; set; }
            }


            public class BatchResponse
            {
                public int id { get; set; }
                public string number { get; set; }
                public string name { get; set; }
            }

            public class JobResponse
            {
                public int id { get; set; }
                public string number { get; set; }
                public string type { get; set; }
            }

            public class RoyaltyResponse
            {
                public string status { get; set; }
                [JsonProperty("date")]
                public DateTime _date { get; set; }
                public DateTime sentOn { get; set; }
                public string memo { get; set; }
            }

            public class EmployeeInfoResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public DateTime modifiedOn { get; set; }
            }
            public class InvoiceResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<InvoiceResponse> data { get; set; }
                public int totalCount { get; set; }
            }
            public class InvoiceItemResponse
            {
                public int id { get; set; }
                public string description { get; set; }
                public string quantity { get; set; }
                public string cost { get; set; }
                public string totalCost { get; set; }
                public string inventoryLocation { get; set; }
                public string price { get; set; }
                public string type { get; set; }
                public string skuName { get; set; }
                public int skuId { get; set; }
                public string total { get; set; }
                public bool inventory { get; set; }
                public bool taxable { get; set; }
                public GLAccountResponse generalLedgerAccount { get; set; }
                public GLAccountResponse costOfSaleAccount { get; set; }
                public GLAccountResponse assetAccount { get; set; }
                public int membershipTypeId { get; set; }
                public ItemGroupResponse itemGroup { get; set; }
                public string displayName { get; set; }
                public decimal soldHours { get; set; }
                public DateTime modifiedOn { get; set; }
                public string serviceDate { get; set; }
                public int order { get; set; }
            }

            public class GLAccountResponse
            {
                public string name { get; set; }
                public string number { get; set; }
                public string type { get; set; }
                public string detailType { get; set; }
            }


            public class ItemGroupResponse
            {
                public int rootId { get; set; }
                public string name { get; set; }
            }

            public class CustomFieldResponse
            {
                public string name { get; set; }
                public string value { get; set; }
            }

            public class MarkInvoiceAsExportedUpdateRequest
            {
                public int invoiceId { get; set; }
                public string externalId { get; set; }
                public string externalMessage { get; set; }
            }
            public class MarkInvoiceAsExportedUpdateResponse
            {
                public int invoiceId { get; set; }
                public bool success { get; set; }
                public string errorMessage { get; set; }
            }
            public class InvoiceUpdateRequest
            {
                public string number { get; set; }
                public int typeId { get; set; }
                public DateTime invoicedOn { get; set; }
                public int subtotal { get; set; }
                public int tax { get; set; }
                public string summary { get; set; }
                public Royaltystatus royaltyStatus { get; set; }
                public DateTime royaltyDate { get; set; }
                public DateTime royaltySentOn { get; set; }
                public string royaltyMemo { get; set; }
                public string exportId { get; set; }
                public List<InvoiceItemUpdateRequest> items { get; set; }
                public List<PaymentSettlementUpdateRequest> payments { get; set; }
            }

            public class CustomFieldUpdateRequest
            {
                public List<CustomFieldOperationRequest> operations { get; set; }
            }

            public class CustomFieldOperationRequest
            {
                public int objectId { get; set; }
                public List<CustomFieldPairRequest> customFields { get; set; }
            }

            public class CustomFieldPairRequest
            {
                public string name { get; set; }
                public string value { get; set; }
            }

            public class PaymentCreateRequest
            {
                public int typeId { get; set; }
                public string memo { get; set; }
                public DateTime paidOn { get; set; }
                public string authCode { get; set; }
                public string checkNumber { get; set; }
                public string exportId { get; set; }
                public PaymentStatus transactionStatus { get; set; }
                public PaymentStatus status { get; set; }
                public List<PaymentSplitApiModel> splits { get; set; }
            }

            public class TransactionProcessingStatus
            {
                public string status { get; set; }
            }

            public class PaymentStatus
            {
                public string status { get; set; }
            }

            public class PaymentSplitApiModel
            {
                public int invoiceId { get; set; }
                public int amount { get; set; }
            }

            public class PaymentResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<PaymentResponse> data { get; set; }
                public int totalCount { get; set; }
            }


            public class PaymentResponse
            {
                public int id { get; set; }
                public int typeId { get; set; }
                public bool active { get; set; }
                public string memo { get; set; }
                public DateTime paidOn { get; set; }
                public string authCode { get; set; }
                public string checkNumber { get; set; }
                public string exportId { get; set; }
                public TransactionProcessingStatus transactionStatus { get; set; }
                public PaymentStatus status { get; set; }
                public List<PaymentSplitApiModel> splits { get; set; }
            }

            public class DetailedPaymentResponse
            {
                public int id { get; set; }
                public string syncStatus { get; set; }
                public string referenceNumber { get; set; }
                [JsonProperty("date")]
                public DateTime _date { get; set; }
                public string type { get; set; }
                public string typeId { get; set; }
                public string total { get; set; }
                public string unappliedAmount { get; set; }
                public string memo { get; set; }
                public NamedFieldResponse customer { get; set; }
                public NamedFieldResponse batch { get; set; }
                public string createdBy { get; set; }
                public GLAccountResponse generalLedgerAccount { get; set; }
                public List<PaymentAppliedResponse> appliedTo { get; set; }
                public List<CustomFieldModel> customFields { get; set; }
                public string authCode { get; set; }
                public string checkNumber { get; set; }
            }

            public class NamedFieldResponse
            {
                public int id { get; set; }
                public string name { get; set; }
            }



            public class PaymentAppliedResponse
            {
                public int appliedTo { get; set; }
                public string appliedAmount { get; set; }
                public DateTime appliedOn { get; set; }
                public string appliedBy { get; set; }
            }

            public class CustomFieldModel
            {
                public string name { get; set; }
                public string value { get; set; }
            }
            public class PaymentStatusBatchRequest
            {
                public PaymentStatus status { get; set; }
                public List<int> paymentIds { get; set; }
            }

            public class PaymentUpdateRequest
            {
                public int typeId { get; set; }
                public bool active { get; set; }
                public string memo { get; set; }
                public string paidOn { get; set; }
                public string authCode { get; set; }
                public string checkNumber { get; set; }
                public string exportId { get; set; }
                public TransactionProcessingStatus transactionStatus { get; set; }
                public string status { get; set; }
                public List<PaymentSplitApiModel> splits { get; set; }
            }

            public class PaymentTermModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public string dueDayType { get; set; }
                public int dueDay { get; set; }
                public bool isCustomerDefault { get; set; }
                public bool isVendorDefault { get; set; }
                public bool active { get; set; }
                public bool inUse { get; set; }
                public PaymentTermPenaltyModel paymentTermPenaltyModel { get; set; }
                public PaymentTermDiscountModel paymentTermDiscountModel { get; set; }
            }

            public class PaymentTermPenaltyModel
            {
                public int id { get; set; }
                public PaymentTermApplyTo penaltyApplyTo { get; set; }
                public int penalty { get; set; }
                public PaymentTermValueType penaltyType { get; set; }
                public int maxPenaltyAmount { get; set; }
                public PaymentTermPenaltyFrequency penaltyFrequency { get; set; }
                public int serviceTaskId { get; set; }
            }


            public class PaymentTermValueType
            {
                public string value { get; set; }
            }

            public class PaymentTermPenaltyFrequency
            {
                public string value { get; set; }
            }

            public class PaymentTermDiscountModel
            {
                public int id { get; set; }
                public PaymentTermApplyTo discountApplyTo { get; set; }
                public int discount { get; set; }
                public PaymentTermValueType discountType { get; set; }
                public string account { get; set; }
                public DiscountAppliedBy applyBy { get; set; }
                public int applyByValue { get; set; }
            }

            public class PaymentTermApplyTo
            {
                public string value { get; set; }
            }

            public class DiscountAppliedBy
            {
                public string value { get; set; }
            }


            public class PaymentTypeResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<PaymentTypeResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class PaymentTypeResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class TaxZoneResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<TaxZoneResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class TaxZoneResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public int color { get; set; }
                public bool isTaxRateSeparated { get; set; }
                public bool isMultipleTaxZone { get; set; }
                public List<TaxRateResponse> rates { get; set; }
                public DateTime createdOn { get; set; }
                public bool active { get; set; }
            }

            public class TaxRateResponse
            {
                public int id { get; set; }
                public string taxName { get; set; }
                public List<string> taxBaseType { get; set; }
                public decimal taxRate { get; set; }
                public string salesTaxItem { get; set; }
            }
        }
        public class CRM
        {
            // UPDATED: 01-17-2022

            public class CreateLocationRequest
            {
                public string name { get; set; }
                public Address address { get; set; }
                public List<Contact> contacts { get; set; }
                public List<Customfield> customFields { get; set; }
                public int customerId { get; set; }
            }

            public class CreatedCustomerResponse
            {
                public int id { get; set; }
                public bool active { get; set; }
                public string name { get; set; }
                public Type type { get; set; }
                public Address address { get; set; }
                public List<Customfield> customFields { get; set; } = new List<Customfield>();
                public int balance { get; set; }
                public bool doNotMail { get; set; }
                public bool doNotService { get; set; }
                public DateTime createdOn { get; set; }
                public int createdById { get; set; }
                public DateTime modifiedOn { get; set; }
                public int mergedToId { get; set; }
                public List<Location> locations { get; set; } = new List<Location>();
                public List<Contact> contacts { get; set; } = new List<Contact>();
            }



            public class BookingResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<BookingResponse> data { get; set; } = new List<BookingResponse>();
            }

            public class BookingResponse
            {
                public int id { get; set; }
                public string source { get; set; }
                public DateTime createdOn { get; set; }
                public string name { get; set; }
                public Address address { get; set; }
                public Customertype customerType { get; set; }
                public DateTime start { get; set; }
                public string summary { get; set; }
                public int campaignId { get; set; }
                public int businessUnitId { get; set; }
                public bool isFirstTimeClient { get; set; }
                public string uploadedImages { get; set; }
                public bool isSendConfirmationEmail { get; set; }
                public BookingStatus status { get; set; }
                public int dismissingReasonId { get; set; }
                public int jobId { get; set; }
                public string externalId { get; set; }
                public Priority priority { get; set; }
                public int jobTypeId { get; set; }
                public int bookingProviderId { get; set; }
            }

            public class Address
            {
                public string street { get; set; }
                public string unit { get; set; }
                public string city { get; set; }
                public string state { get; set; }
                public string zip { get; set; }
                public string country { get; set; }
            }

            public class Customertype
            {
                public string type;
            }

            public class BookingStatus
            {
                public string status;
            }

            public class Priority
            {
                public string priority;
            }

            public class CreateBookingRequest
            {
                public string source { get; set; }
                public string name { get; set; }
                public Address address { get; set; }
                public List<Contact> contacts { get; set; } = new List<Contact>();
                public string customerType { get; set; }
                public DateTime start { get; set; }
                public string summary { get; set; }
                public int campaignId { get; set; }
                public int businessUnitId { get; set; }
                public int jobTypeId { get; set; }
                public Priority priority { get; set; }
                public bool isFirstTimeClient { get; set; }
                public string uploadedImages { get; set; }
                public bool isSendConfirmationEmail { get; set; }
                public string externalId { get; set; }
            }

            public class UpdateBookingRequest
            {
                public string source { get; set; }
                public string name { get; set; }
                public Address address { get; set; }
                public Customertype customerType { get; set; }
                public DateTime start { get; set; }
                public string summary { get; set; }
                public int campaignId { get; set; }
                public int businessUnitId { get; set; }
                public int jobTypeId { get; set; }
                public Priority priority { get; set; }
                public bool isFirstTimeClient { get; set; }
                public string uploadedImages { get; set; }
                public string externalId { get; set; }
            }


            public class Contact
            {
                public string type { get; set; }
                public string value { get; set; }
                public string memo { get; set; }
            }

            public class BookingContactResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<BookingContactResponse> data { get; set; } = new List<BookingContactResponse>();
            }

            public class BookingContactResponse
            {
                public int id { get; set; }
                public ContactType type { get; set; }
                public string value { get; set; }
                public string memo { get; set; }
                public string modifiedOn { get; set; }
            }

            public class ContactType
            {
                public string type { get; set; }
            }

            public class ContactCreateRequest
            {
                public string type { get; set; }
                public string value { get; set; }
                public string memo { get; set; }
            }

            public class CustomerResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<CustomerResponse> data { get; set; } = new List<CustomerResponse>();
            }

            public class CustomerResponse
            {
                public int id { get; set; }
                public bool active { get; set; }
                public string name { get; set; }
                public string type { get; set; }
                public Address address { get; set; }
                public List<Customfield> customFields { get; set; } = new List<Customfield>();
                public decimal balance { get; set; }
                public bool doNotMail { get; set; }
                public bool doNotService { get; set; }
                public DateTime createdOn { get; set; }
                public int createdById { get; set; }
                public DateTime modifiedOn { get; set; }
                public int mergedToId { get; set; }
            }

            public class Type
            {
                public string type { get; set; }
            }



            public class Customfield
            {
                public int typeId { get; set; }
                public string name { get; set; }
                public string value { get; set; }
            }

            public class CreateCustomerRequest
            {
                public string name { get; set; }
                public Type type { get; set; }
                public bool doNotMail { get; set; }
                public bool doNotService { get; set; }
                public List<Location> locations { get; set; } = new List<Location>();
                public Address address { get; set; }
                public List<Contact> contacts { get; set; } = new List<Contact>();
                public List<Customfield> customFields { get; set; } = new List<Customfield>();
            }


            public class Location
            {
                public string name { get; set; }
                public Address address { get; set; }
                public List<Contact> contacts { get; set; } = new List<Contact>();
                public List<Customfield> customFields { get; set; } = new List<Customfield>();
            }

            public class UpdateCustomerRequest
            {
                public string name { get; set; }
                public Type type { get; set; }
                public Address address { get; set; }
                public List<Customfield> customFields { get; set; }
                public bool doNotMail { get; set; }
                public bool doNotService { get; set; }
                public bool active { get; set; }
            }

            public class CustomerContactWithModifiedOnResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<CustomerContactWithModifiedOnResponse> data { get; set; } = new List<CustomerContactWithModifiedOnResponse>();
            }

            public class CustomerContactWithModifiedOnResponse
            {
                public int id { get; set; }
                public string type { get; set; }
                public string value { get; set; }
                public string memo { get; set; }
                public DateTime modifiedOn { get; set; }
                public Phonesettings phoneSettings { get; set; }
            }

            public class Phonesettings
            {
                public string phoneNumber { get; set; }
                public bool doNotText { get; set; }
            }

            public class CreateCustomerContactRequest
            {
                public Type type { get; set; }
                public string value { get; set; }
                public string memo { get; set; }
            }
            public class UpdateCustomerContactRequest
            {
                public Type type { get; set; }
                public string value { get; set; }
                public string memo { get; set; }
            }

            public class NoteResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<NoteResponse> data { get; set; } = new List<NoteResponse>();
            }

            public class NoteResponse
            {
                public string text { get; set; }
                public bool isPinned { get; set; }
                public int createdById { get; set; }
                public DateTime createdOn { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class CreateCustomerNoteRequest
            {
                public string text { get; set; }
                public bool pinToTop { get; set; }
                public bool addToLocations { get; set; }
            }

            public class LocationResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<LocationResponse> data { get; set; } = new List<LocationResponse>();
            }

            public class LocationResponse
            {
                public int id { get; set; }
                public int customerId { get; set; }
                public bool active { get; set; }
                public string name { get; set; }
                public Address address { get; set; }
                public List<Customfield> customFields { get; set; } = new List<Customfield>();
                public DateTime createdOn { get; set; }
                public int createdById { get; set; }
                public DateTime modifiedOn { get; set; }
                public int mergedToId { get; set; }
                public int taxZoneId { get; set; }
            }

            public class CreateLocationResponse
            {
                public int taxZoneId { get; set; }
                public int id { get; set; }
                public int customerId { get; set; }
                public bool active { get; set; }
                public string name { get; set; }
                public Address address { get; set; }
                public List<Customfield> customFields { get; set; }
                public DateTime createdOn { get; set; }
                public int createdById { get; set; }
                public DateTime modifiedOn { get; set; }
                public int mergedToId { get; set; }
                public List<Contact> contacts { get; set; }
            }


            public class UpdateLocationRequest
            {
                public int customerId { get; set; }
                public string name { get; set; }
                public Address address { get; set; }
                public bool active { get; set; }
                public int taxZoneId { get; set; }
                public List<Customfield> customFields { get; set; } = new List<Customfield>();
            }

            public class LocationContactResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<LocationContactResponse> data { get; set; } = new List<LocationContactResponse>();
            }

            public class LocationContactResponse
            {
                public int id { get; set; }
                public Type type { get; set; }
                public string value { get; set; }
                public string memo { get; set; }
                public Phonesettings phoneSettings { get; set; }
                public string modifiedOn { get; set; }
            }

            public class LocationContactUpdateRequest
            {
                public Type type { get; set; }
                public string value { get; set; }
                public string memo { get; set; }
            }


            // END OF UPDATE

            public class LeadResponse
            {
                public int id { get; set; }
                public LeadStatus status { get; set; }
                public int customerId { get; set; }
                public int locationId { get; set; }
                public int businessUnitId { get; set; }
                public int jobTypeId { get; set; }
                public Priority priority { get; set; }
                public int campaignId { get; set; }
                public string summary { get; set; }
                public int callReasonId { get; set; }
                public string latestFollowUpDate { get; set; }
                public DateTime createdOn { get; set; }
                public int createdById { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class LeadStatus
            {
                public string status { get; set; }
            }



            public class LeadResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<LeadResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class TagsResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<TagsResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class TagsResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
            }
        }
        public class Dispatch
        {
            // UPDATED: 1/17/2022

            public class NonJobAppointmentResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<NonJobAppointmentResponse> data { get; set; } = new List<NonJobAppointmentResponse>();
            }

            public class NonJobAppointmentResponse
            {
                public int id { get; set; }
                public int technicianId { get; set; }
                public DateTime start { get; set; }
                public string name { get; set; }
                public string duration { get; set; }
                public int timesheetCodeId { get; set; }
                public string summary { get; set; }
                public bool clearDispatchBoard { get; set; }
                public bool clearTechnicianView { get; set; }
                public bool removeTechnicianFromCapacityPlanning { get; set; }
                public bool allDay { get; set; }
                public bool active { get; set; }
                public DateTime createdOn { get; set; }
                public int createdById { get; set; }
            }

            public class NonJobAppointmentCreateRequest
            {
                public int technicianId { get; set; }
                public DateTime start { get; set; }
                public string duration { get; set; }
                public string name { get; set; }
                public int timesheetCodeId { get; set; }
                public string summary { get; set; }
                public bool clearDispatchBoard { get; set; }
                public bool clearTechnicianView { get; set; }
                public bool removeTechnicianFromCapacityPlanning { get; set; }
            }

            public class NonJobAppointmentUpdateRequest
            {
                public int technicianId { get; set; }
                public DateTime start { get; set; }
                public string duration { get; set; }
                public string name { get; set; }
                public int timesheetCodeId { get; set; }
                public string summary { get; set; }
                public bool clearDispatchBoard { get; set; }
                public bool clearTechnicianView { get; set; }
                public bool removeTechnicianFromCapacityPlanning { get; set; }
            }



            // END OF UPDATE
            public class AppointmentAssignmentResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<AppointmentAssignmentResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class AppointmentAssignmentResponse
            {
                public int id { get; set; }
                public int technicianId { get; set; }
                public string technicianName { get; set; }
                public int assignedById { get; set; }
                public DateTime assignedOn { get; set; }
                public string status { get; set; }
                public bool isPaused { get; set; }
                public int jobId { get; set; }
                public int appointmentId { get; set; }
            }

            public class JobAppointmentAssignmentStatus
            {
                public string status { get; set; }
            }

            public class CapacityQueryFilter
            {
                public DateTime startsOnOrAfter { get; set; }
                public DateTime endsOnOrBefore { get; set; }
                public List<int> businessUnitIds { get; set; }
                public int jobTypeId { get; set; }
                public bool skillBasedAvailability { get; set; }
            }


            public class CapacityResponse
            {
                public string startsOnOrAfter { get; set; }
                public string endsOnOrBefore { get; set; }
                public List<int> businessUnitIds { get; set; }
                public int jobTypeId { get; set; }
                public bool skillBasedAvailability { get; set; }
            }

            public class TechnicianShiftResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<TechnicianShiftResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class TechnicianShiftResponse
            {
                public int id { get; set; }
                public Shifttype shiftType { get; set; }
                public string title { get; set; }
                public string note { get; set; }
                public bool active { get; set; }
                public int technicianId { get; set; }
                public string start { get; set; }
                [JsonProperty("end")]
                public DateTime _end { get; set; }
            }

            public class Shifttype
            {
                public string type { get; set; }
            }
        }
        public class EquipmentSystems
        {
            public class InstalledEquipmentCreateRequest
            {
                public int locationId { get; set; }
                public string name { get; set; }
                public DateTime installedOn { get; set; }
                public string serialNumber { get; set; }
                public string memo { get; set; }
                public string manufacturer { get; set; }
                public string model { get; set; }
                public int cost { get; set; }
                public DateTime manufacturerWarrantyStart { get; set; }
                public DateTime manufacturerWarrantyEnd { get; set; }
                public DateTime serviceProviderWarrantyStart { get; set; }
                public DateTime serviceProviderWarrantyEnd { get; set; }
                public List<CustomFieldRequestModel> customFields { get; set; }
            }

            public class CustomFieldRequestModel
            {
                public int id { get; set; }
                public int typeId { get; set; }
                public string value { get; set; }
            }

            public class InstalledEquipmentDetailedResponse
            {
                public int id { get; set; }
                public int locationId { get; set; }
                public int customerId { get; set; }
                public string name { get; set; }
                public DateTime installedOn { get; set; }
                public string serialNumber { get; set; }
                public string memo { get; set; }
                public string manufacturer { get; set; }
                public string model { get; set; }
                public int cost { get; set; }
                public DateTime manufacturerWarrantyStart { get; set; }
                public DateTime manufacturerWarrantyEnd { get; set; }
                public DateTime serviceProviderWarrantyStart { get; set; }
                public DateTime serviceProviderWarrantyEnd { get; set; }
                public List<CustomFieldResponseModel> customFields { get; set; }
            }

            public class CustomFieldResponseModel
            {
                public int id { get; set; }
                public int typeId { get; set; }
                public string name { get; set; }
                public string value { get; set; }
            }

            public class InstalledEquipmentResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<InstalledEquipmentResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class InstalledEquipmentResponse
            {
                public int id { get; set; }
                public int locationId { get; set; }
                public int customerId { get; set; }
                public string name { get; set; }
                public DateTime installedOn { get; set; }
                public string serialNumber { get; set; }
                public string memo { get; set; }
                public string manufacturer { get; set; }
                public string model { get; set; }
                public int cost { get; set; }
                public DateTime manufacturerWarrantyStart { get; set; }
                public DateTime manufacturerWarrantyEnd { get; set; }
                public DateTime serviceProviderWarrantyStart { get; set; }
                public DateTime serviceProviderWarrantyEnd { get; set; }
            }

            public class InstalledEquipmentUpdateRequest
            {
                public string name { get; set; }
                public DateTime installedOn { get; set; }
                public string serialNumber { get; set; }
                public string memo { get; set; }
                public string manufacturer { get; set; }
                public string model { get; set; }
                public int cost { get; set; }
                public DateTime manufacturerWarrantyStart { get; set; }
                public DateTime manufacturerWarrantyEnd { get; set; }
                public DateTime serviceProviderWarrantyStart { get; set; }
                public DateTime serviceProviderWarrantyEnd { get; set; }
                public List<CustomFieldRequestModel> customFields { get; set; }
            }
        }
        public class Forms {
            public class FormSubmissionResponse {
                public int id {get; set;}
                public int formId {get; set;}
                public string formName {get; set;}
                public DateTime submittedOn {get; set;}
                public int createdById {get; set;}
                public string status {get; set;}
                public List<FormOwner> owners {get; set;}
                public List<FormUnit> units {get; set;} 
            }
            public class FormOwner {
                public string type {get; set;}
                public int id {get; set;}

            }
            public class FormUnit {
                // May be null, content/type depends on unit type
                public object value {get; set;}
                // may be null, content/type depends on unit type
                public List<object> values {get; set;}
                public string name {get; set;}
                public string type {get; set;}
                // Assuming string for now, may not always be?
                public string options {get; set;}
                public string comment {get; set;}
                public List<object> attachments {get; set;}
                // Seems to only be used if the unit type is "Section", may be wrong
                public List<FormUnit> units {get; set;}
            }
            public class FormSubmissionResponse_P {
                public int page {get; set;}
                public int pageSize {get; set;}
                public Boolean hasMore {get; set;}
                public int totalCount {get; set;}
                public List<FormSubmissionResponse> data {get; set;}
            }
        }
        public class Inventory
        {
            
            public class CreatePurchaseOrderResponse
            {
                public int id;
            }

            public class CreatePurchaseOrderRequest
            {
                public int vendorId { get; set; }
                public int typeId { get; set; }
                public int businessUnitId { get; set; }
                public int inventoryLocationId { get; set; }
                public int jobId { get; set; }
                public int technicianId { get; set; }
                public int projectId { get; set; }
                public CreateAddressRequest shipTo { get; set; }
                public string vendorInvoiceNumber { get; set; }
                public bool impactsTechnicianPayroll { get; set; }
                public string memo { get; set; }
                [JsonProperty("date")]
                public DateTime _date { get; set; }
                public DateTime requiredOn { get; set; }
                public int tax { get; set; }
                public int shipping { get; set; }
                public List<CreatePurchaseOrderItemRequest> items { get; set; }
            }

            public class CreateAddressRequest
            {
                public string description { get; set; }
                public AddressRequest address { get; set; }
            }

            public class AddressRequest
            {
                public string street { get; set; }
                public string unit { get; set; }
                public string city { get; set; }
                public string state { get; set; }
                public string zip { get; set; }
                public string country { get; set; }
            }

            public class CreatePurchaseOrderItemRequest
            {
                public int skuId { get; set; }
                public string description { get; set; }
                public string vendorPartNumber { get; set; }
                public int quantity { get; set; }
                public int cost { get; set; }
            }

            public class PurchaseOrderResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<PurchaseOrderResponse> data { get; set; }
                public int totalCount { get; set; }
            }
            public class WarehouseResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<WarehouseResponse> data { get; set; }
                public int totalCount { get; set; }
            }
            public class WarehouseResponse {
                public int id {get; set;}
                public string name {get; set;}
                public Boolean active {get; set;}
                public AddressResponse address;
                public DateTime createdOn;
                public DateTime modifiedOn;

            }
            public class PurchaseOrderResponse
            {
                public int id { get; set; }
                public string number { get; set; }
                public int invoiceId { get; set; }
                public int jobId { get; set; }
                public int projectId { get; set; }
                public string status { get; set; }
                public int typeId { get; set; }
                public int vendorId { get; set; }
                public int technicianId { get; set; }
                public AddressResponse shipTo { get; set; }
                public int businessUnitId { get; set; }
                public int inventoryLocationId { get; set; }
                public int batchId { get; set; }
                public string vendorDocumentNumber { get; set; }
                [JsonProperty("date")]
                public DateTime _date { get; set; }
                public DateTime requiredOn { get; set; }
                public DateTime sentOn { get; set; }
                public DateTime receivedOn { get; set; }
                public DateTime modifiedOn { get; set; }
                public decimal total { get; set; }
                public decimal tax { get; set; }
                public decimal shipping { get; set; }
                public string summary { get; set; }
                public List<PurchaseOrderItemResponse> items { get; set; }
                public List<CustomFieldApiModel> customFields { get; set; }
            }

            public class AddressResponse
            {
                public string street { get; set; }
                public string unit { get; set; }
                public string city { get; set; }
                public string state { get; set; }
                public string zip { get; set; }
                public string country { get; set; }
            }

            public class PurchaseOrderItemResponse
            {
                public int id { get; set; }
                public int skuId { get; set; }
                public string skuName { get; set; }
                public string skuCode { get; set; }
                public string skuType { get; set; }
                public string description { get; set; }
                public string vendorPartNumber { get; set; }
                public decimal quantity { get; set; }
                public decimal quantityReceived { get; set; }
                public decimal cost { get; set; }
                public decimal total { get; set; }
                public List<SerialNumberResponse> serialNumbers { get; set; }
                public string status { get; set; }
                public bool chargeable { get; set; }
            }

            public class SerialNumberResponse
            {
                public int id { get; set; }
                public string number { get; set; }
            }

            public class CustomFieldApiModel
            {
                public int typeId { get; set; }
                public string name { get; set; }
                public string value { get; set; }
            }

            public class UpdatePurchaseOrderRequest
            {
                public int vendorId { get; set; }
                public int typeId { get; set; }
                public int businessUnitId { get; set; }
                public int inventoryLocationId { get; set; }
                public int jobId { get; set; }
                public int technicianId { get; set; }
                public int projectId { get; set; }
                public UpdateAddressRequest shipTo { get; set; }
                public string vendorInvoiceNumber { get; set; }
                public bool impactsTechnicianPayroll { get; set; }
                public string memo { get; set; }
                [JsonProperty("date")]
                public DateTime _date { get; set; }
                public DateTime requiredOn { get; set; }
                public int tax { get; set; }
                public int shipping { get; set; }
                public List<UpdatePurchaseOrderItemRequest> items { get; set; }
                public List<RemovePurchaseOrderItemRequest> removedItems { get; set; }
            }

            public class UpdateAddressRequest
            {
                public string description { get; set; }
                public AddressRequest address { get; set; }
            }

            public class UpdatePurchaseOrderItemRequest
            {
                public int id { get; set; }
                public int skuId { get; set; }
                public string description { get; set; }
                public string vendorPartNumber { get; set; }
                public int quantity { get; set; }
                public int cost { get; set; }
            }

            public class RemovePurchaseOrderItemRequest
            {
                public int id { get; set; }
                public bool doNotReplenish { get; set; }
            }

            public class VendorResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<VendorResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class VendorResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
                public bool isTruckReplenishment { get; set; }
                public bool isMobileCreationRestricted { get; set; }
                public string memo { get; set; }
                public string deliveryOption { get; set; }
                public decimal defaultTaxRate { get; set; }
                public VendorContactInfoResponse contactInfo { get; set; }
                public AddressResponse address { get; set; }
            }

            public class VendorContactInfoResponse
            {
                public string firstName { get; set; }
                public string lastName { get; set; }
                public string phone { get; set; }
                public string email { get; set; }
                public string fax { get; set; }
            }
        }
        public class JobBooking_ContactExperience
        {
            public class CallReasonResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<CallReasonResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class CallReasonResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool isLead { get; set; }
                public bool active { get; set; }
            }
        }
        public class JobPlanning_Management
        {
            public class AppointmentAddRequest
            {
                public int jobId { get; set; }
                public DateTime start { get; set; }
                [JsonProperty("end")]
                public DateTime _end { get; set; }
                public DateTime arrivalWindowStart { get; set; }
                public DateTime arrivalWindowEnd { get; set; }
                public List<int> technicianIds { get; set; }
                public string specialInstructions { get; set; }
            }

            public class AppointmentResponse
            {
                public int id { get; set; }
                public int jobId { get; set; }
                public string appointmentNumber { get; set; }
                public DateTime start { get; set; }
                [JsonProperty("end")]
                public DateTime _end { get; set; }
                public DateTime arrivalWindowStart { get; set; }
                public DateTime arrivalWindowEnd { get; set; }
                public string status { get; set; }
                public string specialInstructions { get; set; }
                public DateTime createdOn { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class Status
            {
                public string status { get; set; }
            }

            public class UpdateAppointmentSpecialInstructionsRequest
            {
                public string specialInstructions { get; set; }
            }

            public class AppointmentResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<AppointmentResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class HoldAppointmentRequest
            {
                public int reasonId { get; set; }
                public string memo { get; set; }
            }

            public class AppointmentRescheduleRequest
            {
                public DateTime start { get; set; }
                [JsonProperty("end")]
                public DateTime _end { get; set; }
                public DateTime arrivalWindowStart { get; set; }
                public DateTime arrivalWindowEnd { get; set; }
            }

            public class JobCancelReasonResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<JobCancelReasonResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class JobCancelReasonResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
            }

            public class JobHoldReasonResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<JobHoldReasonResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class JobHoldReasonResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
            }

            public class CancelJobRequest
            {
                public int reasonId { get; set; }
                public string memo { get; set; }
            }

            public class JobCreateRequest
            {
                public int customerId { get; set; }
                public int locationId { get; set; }
                public int businessUnitId { get; set; }
                public int jobTypeId { get; set; }
                public string priority { get; set; }
                public int campaignId { get; set; }
                public List<AppointmentInformation> appointments { get; set; }
                public string summary { get; set; }
                public List<CustomFieldApiModel> customFields { get; set; }
                public List<int> tagTypeIds { get; set; }
                public ExternalDataUpdateRequest externalData { get; set; }
            }

            public class ExternalDataUpdateRequest
            {
                public string applicationGuid { get; set; }
                public List<ExternalDataModel> externalData { get; set; }
            }

            public class ExternalDataModel
            {
                public string key { get; set; }
                public string value { get; set; }
            }

            public class AppointmentInformation
            {
                public DateTime start { get; set; }
                [JsonProperty("end")]
                public DateTime _end { get; set; }
                public DateTime arrivalWindowStart { get; set; }
                public DateTime arrivalWindowEnd { get; set; }
                public List<int> technicianIds { get; set; }
            }

            public class CustomFieldApiModel
            {
                public int typeId { get; set; }
                public string name { get; set; }
                public string value { get; set; }
            }

            public class JobResponse
            {
                public int id { get; set; }
                public string jobNumber { get; set; }
                public int customerId { get; set; }
                public int locationId { get; set; }
                public string jobStatus { get; set; }
                public DateTime completedOn { get; set; }
                public int businessUnitId { get; set; }
                public int jobTypeId { get; set; }
                public string priority { get; set; }
                public int campaignId { get; set; }
                public string summary { get; set; }
                public List<CustomFieldApiModel> customFields { get; set; }
                public int appointmentCount { get; set; }
                public int firstAppointmentId { get; set; }
                public int lastAppointmentId { get; set; }
                public int recallForId { get; set; }
                public int warrantyId { get; set; }
                public JobGeneratedLeadSource jobGeneratedLeadSource { get; set; }
                public bool noCharge { get; set; }
                public bool notificationsEnabled { get; set; }
                public DateTime createdOn { get; set; }
                public int createdById { get; set; }
                public DateTime modifiedOn { get; set; }
                public List<int> tagTypeIds { get; set; }
                public int leadCallId { get; set; }
                public int bookingId { get; set; }
                public int soldById { get; set; }
                public List<ExternalDataUpdateRequest> externalData { get; set; }
            }

            public class JobGeneratedLeadSource
            {
                public int jobId { get; set; }
                public int employeeId { get; set; }
            }

            public class JobNoteCreateRequest
            {
                public string text { get; set; }
                public bool pinToTop { get; set; }
            }

            public class NoteResponse
            {
                public string text { get; set; }
                public bool isPinned { get; set; }
                public int createdById { get; set; }
                public DateTime createdOn { get; set; }
            }

            public class CancelReasonResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<CancelReasonResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class CancelReasonResponse
            {
                public int jobId { get; set; }
                public int reasonId { get; set; }
                public string name { get; set; }
                public string text { get; set; }
            }

            public class JobHistoryResponse
            {
                public List<JobHistoryItemModel> history { get; set; }
            }

            public class JobHistoryItemModel
            {
                public int id { get; set; }
                public int employeeId { get; set; }
                public string eventType { get; set; }
                [JsonProperty("date")]
                public DateTime _date { get; set; }
                public string usedSchedulingTool { get; set; }
            }

            public class JobResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<JobResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class NoteResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<NoteResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class HoldJobRequest
            {
                public int reasonId { get; set; }
                public string memo { get; set; }
            }

            public class UpdateJobRequest
            {
                public int customerId { get; set; }
                public int locationId { get; set; }
                public int businessUnitId { get; set; }
                public int jobTypeId { get; set; }
                public string priority { get; set; }
                public int campaignId { get; set; }
                public string summary { get; set; }
                public bool shouldUpdateInvoiceItems { get; set; }
                public List<CustomFieldApiModel> customFields { get; set; }
                public List<int> tagIds { get; set; }
                public ExternalDataUpdateRequest externalData { get; set; }
            }

            public class UpdateJobTypeRequest
            {
                public string name { get; set; }
                public ExternalDataUpdateRequest externalData { get; set; }
            }

            public class CreateJobTypeRequest
            {
                public string name { get; set; }
                public ExternalDataUpdateRequest externalData { get; set; }
            }

            public class JobTypeResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public DateTime modifiedOn { get; set; }
                public List<ExternalDataUpdateRequest> externalData { get; set; }
            }

            public class JobTypeResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<JobTypeResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class ProjectResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<ProjectResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class ProjectResponse
            {
                public int id { get; set; }
                public string number { get; set; }
                public string name { get; set; }
                public string summary { get; set; }
                public int customerId { get; set; }
                public int locationId { get; set; }
                public DateTime startDate { get; set; }
                public DateTime targetCompletionDate { get; set; }
                public DateTime actualCompletionDate { get; set; }
                public List<CustomFieldApiModel> customFields { get; set; }
            }
        }
        public class Marketing
        {
            public class CampaignCategoryCreateUpdateModel
            {
                public string name { get; set; }
                public bool active { get; set; }
            }

            public class CreateCostRequest
            {
                public int campaignId { get; set; }
                public int year { get; set; }
                public int month { get; set; }
                public int dailyCost { get; set; }
            }

            public class CampaignCostModel
            {
                public int id { get; set; }
                public int year { get; set; }
                public int month { get; set; }
                public int dailyCost { get; set; }
            }

            public class UpdateCostRequest
            {
                public int id { get; set; }
                public int dailyCost { get; set; }
            }

            public class CampaignCreateModel
            {
                public string name { get; set; }
                public int businessUnitId { get; set; }
                public string dnis { get; set; }
                public int cost { get; set; }
                public int categoryId { get; set; }
                public bool active { get; set; }
            }

            public class DetailedCampaignModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public DateTime modifiedOn { get; set; }
                public bool active { get; set; }
                public DetailedCampaignCategoryModel category { get; set; }
            }

            public class DetailedCampaignCategoryModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
            }

            public class CampaignModel_P
            {
                public List<CampaignModel> data { get; set; }
                public int page { get; set; }
                public int pageSize { get; set; }
                public int totalCount { get; set; }
                public bool hasMore { get; set; }
            }

            public class CampaignModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public DateTime modifiedOn { get; set; }
                public bool active { get; set; }
                public CampaignCategoryModel category { get; set; }
            }

            public class CampaignCategoryModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
            }

            public class DetailedCampaignModel_R
            {
                public DetailedCampaignModel data { get; set; }
            }
        }
        public class Memberships
        {
            public class MembershipSaleInvoiceCreateRequest
            {
                public int customerId { get; set; }
                public int businessUnitId { get; set; }
                public int saleTaskId { get; set; }
                public int durationBillingId { get; set; }
                public int locationId { get; set; }
                public string recurringServiceAction { get; set; }
                public int recurringLocationId { get; set; }
            }

            public class MembershipSaleInvoiceCreateResponse
            {
                public int invoiceId { get; set; }
                public int customerMembershipId { get; set; }
            }

            public class CustomerMembershipResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<CustomerMembershipResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class CustomerMembershipResponse
            {
                public int id { get; set; }
                public DateTime followUpOn { get; set; }
                public DateTime modifiedOn { get; set; }
                public DateTime cancellationDate { get; set; }
                public DateTime createdOn { get; set; }
                public DateTime from { get; set; }
                public DateTime nextScheduledBillDate { get; set; }
                [JsonProperty("to")]
                public DateTime _to { get; set; }
                public string billingFrequency { get; set; }
                public string renewalBillingFrequency { get; set; }
                public string status { get; set; }
                public string followUpStatus { get; set; }
                public bool active { get; set; }
                public decimal initialDeferredRevenue { get; set; }
                public int duration { get; set; }
                public int renewalDuration { get; set; }
                public int businessUnitId { get; set; }
                public int customerId { get; set; }
                public int membershipTypeId { get; set; }
                public int activatedById { get; set; }
                public int activatedFromId { get; set; }
                public int billingTemplateId { get; set; }
                public int cancellationBalanceInvoiceId { get; set; }
                public int cancellationInvoiceId { get; set; }
                public int createdById { get; set; }
                public int followUpCustomStatusId { get; set; }
                public int locationId { get; set; }
                public int paymentMethodId { get; set; }
                public int paymentTypeId { get; set; }
                public int recurringLocationId { get; set; }
                public int renewalMembershipTaskId { get; set; }
                public int renewedById { get; set; }
                public int soldById { get; set; }
                public string customerPo { get; set; }
                public string importId { get; set; }
                public string memo { get; set; }
            }





            

            public class CustomerMembershipUpdateRequest
            {
                public int businessUnitId { get; set; }
                public DateTime nextScheduledBillDate { get; set; }
                public string status { get; set; }
                public string memo { get; set; }
                public DateTime from { get; set; }
                [JsonProperty("to")]
                public DateTime _to { get; set; }
                public int soldById { get; set; }
                public int billingTemplateId { get; set; }
                public int locationId { get; set; }
                public RecurringServiceAction recurringServiceAction { get; set; }
                public int recurringLocationId { get; set; }
                public int paymentMethodId { get; set; }
                public int paymentTypeId { get; set; }
                public int renewalMembershipTaskId { get; set; }
                public decimal initialDeferredRevenue { get; set; }
                public int cancellationBalanceInvoiceId { get; set; }
                public int cancellationInvoiceId { get; set; }
                public bool active { get; set; }
                public int duration { get; set; }
            }



            public class RecurringServiceAction
            {
                public string action { get; set; }
            }

            public class InvoiceTemplateCreateRequest
            {
                public string name { get; set; }
                public List<InvoiceTemplateItemCreateRequest> items { get; set; }
            }

            public class InvoiceTemplateItemCreateRequest
            {
                public int skuId { get; set; }
                public int quantity { get; set; }
                public int unitPrice { get; set; }
                public bool isAddOn { get; set; }
                public int workflowActionItemId { get; set; }
                public string description { get; set; }
                public int cost { get; set; }
                public int hours { get; set; }
            }
            public class ModificationResponse
            {
                public int id { get; set; }
            }

            public class InvoiceTemplateResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<InvoiceTemplateResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class InvoiceTemplateResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
                public int total { get; set; }
                public bool isSettingsTemplate { get; set; }
                public string importId { get; set; }
                public List<InvoiceTemplateItemResponse> items { get; set; }
            }

            public class InvoiceTemplateItemResponse
            {
                public int id { get; set; }
                public int skuId { get; set; }
                public SkuType skuType { get; set; }
                public int quantity { get; set; }
                public int unitPrice { get; set; }
                public bool isAddOn { get; set; }
                public string importId { get; set; }
                public int workflowActionItemId { get; set; }
                public string description { get; set; }
                public int cost { get; set; }
                public int hours { get; set; }
            }

            public class SkuType
            {
                public string type { get; set; }
            }

            public class InvoiceTemplateUpdateRequest
            {
                public string name { get; set; }
                public DateTime createdOn { get; set; }
                public int createdById { get; set; }
                public bool active { get; set; }
                public List<InvoiceTemplateItemUpdateRequest> items { get; set; }
            }

            public class InvoiceTemplateItemUpdateRequest
            {
                public int id { get; set; }
                public int skuId { get; set; }
                public int quantity { get; set; }
                public int unitPrice { get; set; }
                public bool isAddOn { get; set; }
                public string description { get; set; }
                public int cost { get; set; }
                public int hours { get; set; }
            }

            public class LocationRecurringServiceEventResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<LocationRecurringServiceEventResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class LocationRecurringServiceEventResponse
            {
                public int id { get; set; }
                public int locationRecurringServiceId { get; set; }
                public string locationRecurringServiceName { get; set; }
                public int membershipId { get; set; }
                public string membershipName { get; set; }
                public string status { get; set; }
                [JsonProperty("date")]
                public DateTime _date { get; set; }
                public DateTime createdOn { get; set; }
            }

            public class MarkEventCompletedStatusUpdateRequest
            {
                public int jobId { get; set; }
            }

            public class LocationRecurringServiceResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<LocationRecurringServiceResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class LocationRecurringServiceResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
                public DateTime createdOn { get; set; }
                public int createdById { get; set; }
                public DateTime modifiedOn { get; set; }
                public string importId { get; set; }
                public int membershipId { get; set; }
                public int locationId { get; set; }
                public int recurringServiceTypeId { get; set; }
                public ServiceRecurrenceDuration durationType { get; set; }
                public int durationLength { get; set; }
                public DateTime from { get; set; }
                [JsonProperty("to")]
                public DateTime _to { get; set; }
                public string memo { get; set; }
                public int invoiceTemplateId { get; set; }
                public int invoiceTemplateForFollowingYearsId { get; set; }
                public bool firstVisitComplete { get; set; }
                public int activatedFromId { get; set; }
                public int allocation { get; set; }
                public int businessUnitId { get; set; }
                public int jobTypeId { get; set; }
                public int campaignId { get; set; }
                public Priority priority { get; set; }
                public string jobSummary { get; set; }
                public ServiceRecurrenceType recurrenceType { get; set; }
                public int recurrenceInterval { get; set; }
                public List<string> recurrenceMonths { get; set; }
                public List<string> recurrenceDaysOfWeek { get; set; }
                public WeekDay recurrenceWeek { get; set; }
                public DayOfWeek recurrenceDayOfNthWeek { get; set; }
                public List<int> recurrenceDaysOfMonth { get; set; }
                public string jobStartTime { get; set; }
                public int estimatedPayrollCost { get; set; }
            }

            public class ServiceRecurrenceDuration
            {
                public string duration { get; set; }
            }

            public class Priority
            {
                public string priority { get; set; }
            }

            public class ServiceRecurrenceType
            {
                public string type { get; set; }
            }

            public class WeekDay
            {
                [JsonProperty("WeekDay")]
                public string WeekDayProp { get; set; }
            }

            public class DayOfWeek
            {
                [JsonProperty("DayOfWeek")]
                public string DayOfWeekProp { get; set; }
            }

            public class LocationRecurringServiceUpdateRequest
            {
                public string name { get; set; }
                public bool active { get; set; }
                public int recurringServiceTypeId { get; set; }
                public string durationType { get; set; }
                public int durationLength { get; set; }
                public DateTime from { get; set; }
                public string memo { get; set; }
                public int invoiceTemplateId { get; set; }
                public int invoiceTemplateForFollowingYearsId { get; set; }
                public int businessUnitId { get; set; }
                public int jobTypeId { get; set; }
                public int campaignId { get; set; }
                public string priority { get; set; }
                public string jobSummary { get; set; }
                public string recurrenceType { get; set; }
                public int recurrenceInterval { get; set; }
                public List<string> recurrenceMonths { get; set; }
                public List<string> recurrenceDaysOfWeek { get; set; }
                public string recurrenceWeek { get; set; }
                public DayOfWeek recurrenceDayOfNthWeek { get; set; }
                public string jobStartTime { get; set; }
                public int estimatedPayrollCost { get; set; }
            }

            public class MembershipTypeResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<MembershipTypeResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class MembershipTypeResponse
            {
                public string name { get; set; }
                public bool active { get; set; }
                public DiscountMode discountMode { get; set; }
                public MembershipLocationTarget locationTarget { get; set; }
                public RevenueRecognitionMode revenueRecognitionMode { get; set; }
                public bool autoCalculateInvoiceTemplates { get; set; }
                public bool useMembershipPricingTable { get; set; }
                public bool showMembershipSavings { get; set; }
                public int id { get; set; }
                public DateTime createdOn { get; set; }
                public int createdById { get; set; }
                public DateTime modifiedOn { get; set; }
                public string importId { get; set; }
                public int billingTemplateId { get; set; }
            }

            public class DiscountMode
            {
                public string mode { get; set; }
            }

            public class MembershipLocationTarget
            {
                public string target { get; set; }
            }

            public class RevenueRecognitionMode
            {
                public string mode { get; set; }
            }



            public class MembershipTypeDiscountItemResponse
            {
                public int id { get; set; }
                public int targetId { get; set; }
                public int discount { get; set; }
            }

            public class MembershipTypeDurationBillingItemResponse
            {
                public int id { get; set; }
                public int duration { get; set; }
                public string billingFrequency { get; set; }
                public int salePrice { get; set; }
                public int billingPrice { get; set; }
                public int renewalPrice { get; set; }
                public string importId { get; set; }
                public bool active { get; set; }
            }



            public class MembershipTypeRecurringServiceItemResponse
            {
                public int id { get; set; }
                public int membershipTypeId { get; set; }
                public int recurringServiceTypeId { get; set; }
                public int offset { get; set; }
                public OffsetType offsetType { get; set; }
                public int allocation { get; set; }
                public string importId { get; set; }
            }

            public class OffsetType
            {
                public string type { get; set; }
            }

            public class RecurringServiceTypeResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<RecurringServiceTypeResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class RecurringServiceTypeResponse
            {
                public int id { get; set; }
                public bool active { get; set; }
                public ServiceRecurrenceType recurrenceType { get; set; }
                public int recurrenceInterval { get; set; }
                public List<string> recurrenceMonths { get; set; }
                public ServiceRecurrenceDuration durationType { get; set; }
                public int durationLength { get; set; }
                public int invoiceTemplateId { get; set; }
                public int businessUnitId { get; set; }
                public int jobTypeId { get; set; }
                public Priority priority { get; set; }
                public int campaignId { get; set; }
                public string jobSummary { get; set; }
                public string name { get; set; }
                public string importId { get; set; }
            }
        }
        public class Payroll
        {
            public class ModificationResponse
            {
                public int id;
            }

            public class PayrollActivityCodeResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<PayrollActivityCodeResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class PayrollActivityCodeResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public string code { get; set; }
                public string earningCategory { get; set; }
            }

            public class PayrollEarningCategory
            {
                public string category { get; set; }
            }

            public class GrossPayItemCreateRequest
            {
                public int payrollId { get; set; }
                public decimal amount { get; set; }
                public int activityCodeId { get; set; }
                [JsonProperty("date")]
                public DateTime _date { get; set; }
                public int invoiceId { get; set; }
            }

            public class GrossPayItemResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<GrossPayItemResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class GrossPayItemResponse
            {
                public int id { get; set; }
                public int employeeId { get; set; }
                public string employeeType { get; set; }
                public string businessUnitName { get; set; }
                public int payrollId { get; set; }
                [JsonProperty("date")]
                public DateTime _date { get; set; }
                public string activity { get; set; }
                public int activityCodeId { get; set; }
                public string activityCode { get; set; }
                public decimal amount { get; set; }
                public decimal amountAdjustment { get; set; }
                public string payoutBusinessUnitName { get; set; }
                public string grossPayItemType { get; set; }
                public DateTime startedOn { get; set; }
                public DateTime endedOn { get; set; }
                public decimal paidDurationHours { get; set; }
                public string paidTimeType { get; set; }
                public int jobId { get; set; }
                public string jobNumber { get; set; }
                public string jobTypeName { get; set; }
                public string projectNumber { get; set; }
                public int projectId { get; set; }
                public int invoiceId { get; set; }
                public string invoiceNumber { get; set; }
                public int invoiceItemId { get; set; }
                public int customerId { get; set; }
                public string customerName { get; set; }
                public int locationId { get; set; }
                public string locationName { get; set; }
                public string locationAddress { get; set; }
                public string locationZip { get; set; }
                public string zoneName { get; set; }
                public string taxZoneName { get; set; }
                public int laborTypeId { get; set; }
                public string laborTypeCode { get; set; }
                public bool isPrevailingWageJob { get; set; }
            }



            public class GrossPayItemType
            {
                public string type { get; set; }
            }

            public class PaidTimeType
            {
                public string type { get; set; }
            }

            public class GrossPayItemUpdateRequest
            {
                public int payrollId { get; set; }
                public int amount { get; set; }
                public int activityCodeId { get; set; }
                [JsonProperty("date")]
                public DateTime _date { get; set; }
                public int invoiceId { get; set; }
            }

            public class JobSplitResponse
            {
                public int jobId { get; set; }
                public int technicianId { get; set; }
                public int split { get; set; }
            }

            public class PayrollAdjustmentCreateRequest
            {
                public string employeeType { get; set; }
                public int employeeId { get; set; }
                public DateTime postedOn { get; set; }
                public int amount { get; set; }
                public string memo { get; set; }
                public int activityCodeId { get; set; }
                public int hours { get; set; }
                public int rate { get; set; }
            }

            public class PayrollAdjustmentResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<PayrollAdjustmentResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class PayrollAdjustmentResponse
            {
                public int id { get; set; }
                public string employeeType { get; set; }
                public int employeeId { get; set; }
                public DateTime postedOn { get; set; }
                public int amount { get; set; }
                public string memo { get; set; }
                public int activityCodeId { get; set; }
                public int hours { get; set; }
                public int rate { get; set; }
            }

            public class PayrollResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<PayrollResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class PayrollResponse
            {
                public int id { get; set; }
                public DateTime startedOn { get; set; }
                public DateTime endedOn { get; set; }
                public int employeeId { get; set; }
                public string employeeType { get; set; }
                public string status { get; set; }
            }



            public class PayrollStatus
            {
                public string status { get; set; }
            }


            public class TimesheetCodeResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<TimesheetCodeResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class TimesheetCodeResponse
            {
                public int id { get; set; }
                public string code { get; set; }
                public string description { get; set; }
                public string type { get; set; }
                public TimesheetCodeEmployeeType applicableEmployeeType { get; set; }
                public TimesheetCodeRateInfoResponse rateInfo { get; set; }
            }

            public class TimesheetCodeType
            {
                public string type { get; set; }
            }

            public class TimesheetCodeEmployeeType
            {
                public string type { get; set; }
            }

            public class TimesheetCodeRateInfoResponse
            {
                public TimesheetHourlyRateType hourlyRate { get; set; }
                public int customHourlyRate { get; set; }
                public int rateMultiplier { get; set; }
            }

            public class TimesheetHourlyRateType
            {
                public string type { get; set; }
            }
        }
        public class Pricebook
        {
            // TODO: Category_Get and Category_GetList
            public class CategoryCreateRequest
            {
                public string name { get; set; }
                public bool active { get; set; }
                public string description { get; set; }
                public int parentId { get; set; }
                public int position { get; set; }
                public string image { get; set; }
                public string categoryType { get; set; }
                public List<int> businessUnitIds { get; set; }
                public List<string> skuImages { get; set; }
                public List<string> skuVideos { get; set; }
            }
            public class CategoryResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<CategoryResponse> data { get; set; }
            }
            public class CategoryResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
                public string description { get; set; }
                public string image { get; set; }
                public int parentId { get; set; }
                public int position { get; set; }
                public CategoryType categoryType { get; set; }
                public CategoryResponse subcategories { get; set; }
                public List<int> businessUnitIds { get; set; }
                public List<string> skuImages { get; set; }
                public List<string> skuVideos { get; set; }
                public string source { get; set; }
                public string externalId { get; set; }
            }
            public class CategoryType
            {
                public string type { get; set; }
            }
            public class CategoryUpdateRequest
            {
                public string name { get; set; }
                public bool active { get; set; }
                public string description { get; set; }
                public int parentId { get; set; }
                public int position { get; set; }
                public string image { get; set; }
                public string categoryType { get; set; }
                public List<int> businessUnitIds { get; set; }
                public List<string> skuImages { get; set; }
                public List<string> skuVideos { get; set; }
            }

            public class DiscountAndFeesCreateRequest
            {
                public string type { get; set; }
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public string amountType { get; set; }
                public int amount { get; set; }
                public int limit { get; set; }
                public bool taxable { get; set; }
                public List<int> categories { get; set; }
                public int hours { get; set; }
                public List<SkuAssetResponse> assets { get; set; }
                public string account { get; set; }
                public string crossSaleGroup { get; set; }
                public bool active { get; set; }
                public int bonus { get; set; }
                public int commissionBonus { get; set; }
                public bool paysCommission { get; set; }
                public bool excludeFromPayroll { get; set; }
            }

            public class SkuAssetResponse
            {
                public string type { get; set; }
                [JsonProperty("alias")]
                public string _alias { get; set; }
                public string url { get; set; }
            }

            public class DiscountAndFeesResponse
            {
                public int id { get; set; }
                public string type { get; set; }
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public string amountType { get; set; }
                public int amount { get; set; }
                public int limit { get; set; }
                public bool taxable { get; set; }
                public List<int> categories { get; set; }
                public int hours { get; set; }
                public List<SkuAssetResponse> assets { get; set; }
                public string account { get; set; }
                public string crossSaleGroup { get; set; }
                public bool active { get; set; }
                public int bonus { get; set; }
                public int commissionBonus { get; set; }
                public bool paysCommission { get; set; }
                public bool excludeFromPayroll { get; set; }
            }

            public class DiscountAndFeesResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<DiscountAndFeesResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class DiscountAndFeesUpdateRequest
            {
                public string type { get; set; }
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public string amountType { get; set; }
                public int amount { get; set; }
                public int limit { get; set; }
                public bool taxable { get; set; }
                public List<int> categories { get; set; }
                public int hours { get; set; }
                public List<SkuAssetRequest> assets { get; set; }
                public string account { get; set; }
                public string crossSaleGroup { get; set; }
                public bool active { get; set; }
                public int bonus { get; set; }
                public int commissionBonus { get; set; }
                public bool paysCommission { get; set; }
                public bool excludeFromPayroll { get; set; }
            }

            public class SkuAssetRequest
            {
                public string type { get; set; }
                [JsonProperty("alias")]
                public string _alias { get; set; }
                public string url { get; set; }
            }

            public class EquipmentCreateRequest
            {
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public int price { get; set; }
                public int memberPrice { get; set; }
                public int addOnPrice { get; set; }
                public int addOnMemberPrice { get; set; }
                public bool active { get; set; }
                public string manufacturer { get; set; }
                public string model { get; set; }
                public SkuWarrantyRequest manufacturerWarranty { get; set; }
                public SkuWarrantyRequest serviceProviderWarranty { get; set; }
                public List<SkuAssetRequest> assets { get; set; }
                public List<EquipmentRecommendationResponse> recommendations { get; set; }
                public List<int> upgrades { get; set; }
                public List<SkuLinkResponse> equipmentMaterials { get; set; }
                public List<int> categories { get; set; }
                public SkuVendorResponse primaryVendor { get; set; }
                public List<SkuVendorResponse> otherVendors { get; set; }
                public string account { get; set; }
                public string costOfSaleAccount { get; set; }
                public string assetAccount { get; set; }
                public string crossSaleGroup { get; set; }
                public bool paysCommission { get; set; }
                public int commissionBonus { get; set; }
                public int hours { get; set; }
                public bool taxable { get; set; }
                public int cost { get; set; }
                public string unitOfMeasure { get; set; }
                public bool isInventory { get; set; }
            }

            public class SkuWarrantyRequest
            {
                public int duration { get; set; }
                public string description { get; set; }
            }


            public class SkuVendorResponse
            {
                public int vendorId { get; set; }
                public string memo { get; set; }
                public string vendorPart { get; set; }
                public decimal cost { get; set; }
                public bool active { get; set; }
                public SkuVendorSubAccountResponse primarySubAccount { get; set; }
                public List<SkuVendorSubAccountResponse> otherSubAccounts { get; set; }
            }


            public class SkuVendorSubAccountResponse
            {
                public decimal cost { get; set; }
                public string accountName { get; set; }
            }

            public class EquipmentRecommendationResponse
            {
                public int skuId { get; set; }
                public string type { get; set; }
            }

            public class SkuLinkResponse
            {
                public int skuId { get; set; }
                public int quantity { get; set; }
            }
            public class EquipmentResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<EquipmentResponse> data { get; set; }
            }

            public class EquipmentResponse
            {
                public int id { get; set; }
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public bool active { get; set; }
                public int price { get; set; }
                public int memberPrice { get; set; }
                public int addOnPrice { get; set; }
                public int addOnMemberPrice { get; set; }
                public string manufacturer { get; set; }
                public string model { get; set; }
                public SkuWarrantyResponse manufacturerWarranty { get; set; }
                public SkuWarrantyResponse serviceProviderWarranty { get; set; }
                public List<int> categories { get; set; }
                public List<SkuAssetResponse> assets { get; set; }
                public List<EquipmentRecommendationResponse> recommendations { get; set; }
                public List<int> upgrades { get; set; }
                public List<SkuLinkResponse> equipmentMaterials { get; set; }
                public SkuVendorResponse primaryVendor { get; set; }
                public List<SkuVendorResponse> otherVendors { get; set; }
                public string account { get; set; }
                public string costOfSaleAccount { get; set; }
                public string assetAccount { get; set; }
                public string crossSaleGroup { get; set; }
                public bool paysCommission { get; set; }
                public int commissionBonus { get; set; }
                public int hours { get; set; }
                public bool taxable { get; set; }
                public int cost { get; set; }
                public string unitOfMeasure { get; set; }
                public bool isInventory { get; set; }
                public DateTime modifiedOn { get; set; }
                public string source { get; set; }
                public string externalId { get; set; }
            }

            public class SkuWarrantyResponse
            {
                public int duration { get; set; }
                public string description { get; set; }
            }

            public class EquipmentUpdateRequest
            {
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public int price { get; set; }
                public int memberPrice { get; set; }
                public int addOnPrice { get; set; }
                public int addOnMemberPrice { get; set; }
                public bool active { get; set; }
                public string manufacturer { get; set; }
                public string model { get; set; }
                public SkuWarrantyRequest manufacturerWarranty { get; set; }
                public SkuWarrantyRequest serviceProviderWarranty { get; set; }
                public List<SkuAssetRequest> assets { get; set; }
                public List<SkuLinkRequest> recommendations { get; set; }
                public List<int> upgrades { get; set; }
                public List<SkuLinkRequest> equipmentMaterials { get; set; }
                public List<int> categories { get; set; }
                public SkuVendorRequest primaryVendor { get; set; }
                public List<SkuVendorRequest> otherVendors { get; set; }
                public string account { get; set; }
                public string costOfSaleAccount { get; set; }
                public string assetAccount { get; set; }
                public string crossSaleGroup { get; set; }
                public bool paysCommission { get; set; }
                public int commissionBonus { get; set; }
                public int hours { get; set; }
                public bool taxable { get; set; }
                public int cost { get; set; }
                public string unitOfMeasure { get; set; }
                public bool isInventory { get; set; }
            }

            public class Manufacturerwarranty
            {
                public int duration { get; set; }
                public string description { get; set; }
            }

            public class Serviceproviderwarranty
            {
                public int duration { get; set; }
                public string description { get; set; }
            }

            public class SkuVendorRequest
            {
                public int vendorId { get; set; }
                public string memo { get; set; }
                public string vendorPart { get; set; }
                public decimal cost { get; set; }
                public bool active { get; set; }
                public SkuVendorSubAccountRequest primarySubAccount { get; set; }
                public List<SkuVendorSubAccountRequest> otherSubAccounts { get; set; }
            }

            public class SkuVendorSubAccountRequest
            {
                public decimal cost { get; set; }
                public string accountName { get; set; }
            }




            public class SkuLinkRequest
            {
                public int skuId { get; set; }
                public string type { get; set; }
            }

            public class MaterialCreateRequest
            {
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public decimal cost { get; set; }
                public bool active { get; set; }
                public decimal price { get; set; }
                public decimal memberPrice { get; set; }
                public decimal addOnPrice { get; set; }
                public decimal addOnMemberPrice { get; set; }
                public decimal hours { get; set; }
                public decimal commissionBonus { get; set; }
                public bool paysCommission { get; set; }
                public bool deductAsJobCost { get; set; }
                public string unitOfMeasure { get; set; }
                public bool isInventory { get; set; }
                public string account { get; set; }
                public string costOfSaleAccount { get; set; }
                public string assetAccount { get; set; }
                public bool taxable { get; set; }
                public SkuVendorRequest primaryVendor { get; set; }
                public List<SkuVendorRequest> otherVendors { get; set; }
                public List<SkuAssetResponse> assets { get; set; }
                public List<int> categories { get; set; }
            }

            public class MaterialResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<MaterialResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class MaterialResponse
            {
                public int id { get; set; }
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public decimal cost { get; set; }
                public bool active { get; set; }
                public decimal price { get; set; }
                public decimal memberPrice { get; set; }
                public decimal addOnPrice { get; set; }
                public decimal addOnMemberPrice { get; set; }
                public decimal hours { get; set; }
                public decimal commissionBonus { get; set; }
                public bool paysCommission { get; set; }
                public bool deductAsJobCost { get; set; }
                public string unitOfMeasure { get; set; }
                public bool isInventory { get; set; }
                public string account { get; set; }
                public string costOfSaleAccount { get; set; }
                public string assetAccount { get; set; }
                public bool taxable { get; set; }
                public SkuVendorResponse primaryVendor { get; set; }
                public List<SkuVendorResponse> otherVendors { get; set; }
                public List<int> categories { get; set; }
                public List<SkuAssetResponse> assets { get; set; }
                public DateTime modifiedOn { get; set; }
                public string source { get; set; }
                public string externalId { get; set; }
            }

            public class MaterialUpdateRequest
            {
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public decimal cost { get; set; }
                public bool active { get; set; }
                public decimal price { get; set; }
                public decimal memberPrice { get; set; }
                public decimal addOnPrice { get; set; }
                public decimal addOnMemberPrice { get; set; }
                public decimal hours { get; set; }
                public decimal commissionBonus { get; set; }
                public bool paysCommission { get; set; }
                public bool deductAsJobCost { get; set; }
                public string unitOfMeasure { get; set; }
                public bool isInventory { get; set; }
                public string account { get; set; }
                public string costOfSaleAccount { get; set; }
                public string assetAccount { get; set; }
                public bool taxable { get; set; }
                public SkuVendorRequest primaryVendor { get; set; }
                public List<SkuVendorRequest> otherVendors { get; set; }
                public List<SkuAssetResponse> assets { get; set; }
                public List<int> categories { get; set; }
            }


            public class ServiceCreateRequest
            {
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public SkuWarrantyRequest warranty { get; set; }
                public List<int> categories { get; set; }
                public int price { get; set; }
                public int memberPrice { get; set; }
                public int addOnPrice { get; set; }
                public int addOnMemberPrice { get; set; }
                public bool taxable { get; set; }
                public string account { get; set; }
                public int hours { get; set; }
                public bool isLabor { get; set; }
                public List<int> recommendations { get; set; }
                public List<int> upgrades { get; set; }
                public List<SkuAssetResponse> assets { get; set; }
                public List<SkuLinkResponse> serviceMaterials { get; set; }
                public List<SkuLinkResponse> serviceEquipment { get; set; }
                public bool active { get; set; }
                public string crossSaleGroup { get; set; }
                public bool paysCommission { get; set; }
                public decimal commissionBonus { get; set; }
            }

            public class ServiceResponse
            {
                public int id { get; set; }
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public SkuWarrantyResponse warranty { get; set; }
                public List<SkuCategoryResponse> categories { get; set; }
                public int price { get; set; }
                public int memberPrice { get; set; }
                public int addOnPrice { get; set; }
                public int addOnMemberPrice { get; set; }
                public bool taxable { get; set; }
                public string account { get; set; }
                public int hours { get; set; }
                public bool isLabor { get; set; }
                public List<int> recommendations { get; set; }
                public List<int> upgrades { get; set; }
                public List<SkuAssetResponse> assets { get; set; }
                public List<SkuLinkResponse> serviceMaterials { get; set; }
                public List<SkuLinkResponse> serviceEquipment { get; set; }
                public bool active { get; set; }
                public string crossSaleGroup { get; set; }
                public bool paysCommission { get; set; }
                public DateTime modifiedOn { get; set; }
                public string source { get; set; }
                public string externalId { get; set; }
            }



            public class SkuCategoryResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
            }

            public class ServiceResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<ServiceResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class ServiceUpdateRequest
            {
                public string code { get; set; }
                public string displayName { get; set; }
                public string description { get; set; }
                public SkuWarrantyRequest warranty { get; set; }
                public List<int> categories { get; set; }
                public int price { get; set; }
                public int memberPrice { get; set; }
                public int addOnPrice { get; set; }
                public int addOnMemberPrice { get; set; }
                public bool taxable { get; set; }
                public string account { get; set; }
                public int hours { get; set; }
                public bool isLabor { get; set; }
                public List<int> recommendations { get; set; }
                public List<int> upgrades { get; set; }
                public List<SkuAssetResponse> assets { get; set; }
                public List<SkuLinkResponse> serviceMaterials { get; set; }
                public List<SkuLinkResponse> serviceEquipment { get; set; }
                public bool active { get; set; }
                public string crossSaleGroup { get; set; }
                public bool paysCommission { get; set; }
                public int commissionBonus { get; set; }
            }
        }
        public class SalesAndEstimates
        {
            public class CreateEstimateRequest
            {
                public string name { get; set; }
                public string summary { get; set; }
                public int tax { get; set; }
                public List<EstimateItemCreateUpdateRequest> items { get; set; }
                public List<ExternalLinkInModel> externalLinks { get; set; }
                public int jobId { get; set; }
            }

            public class EstimateItemUpdateResponse
            {
                public int id { get; set; }
                public SkuModel sku { get; set; }
                public string skuAccount { get; set; }
                public string description { get; set; }
                public int qty { get; set; }
                public int unitRate { get; set; }
                public int total { get; set; }
                public string itemGroupName { get; set; }
                public int itemGroupRootId { get; set; }
                public DateTime modifiedOn { get; set; }
                public int estimateId { get; set; }
            }



            public class EstimateItemCreateUpdateRequest
            {
                public int id { get; set; }
                public int skuId { get; set; }
                public string skuName { get; set; }
                public int parentItemId { get; set; }
                public string description { get; set; }
                public bool isAddOn { get; set; }
                public int quantity { get; set; }
                public int unitPrice { get; set; }
                public bool skipUpdatingMembershipPrices { get; set; }
                public string itemGroupName { get; set; }
                public int itemGroupRootId { get; set; }
            }

            public class ExternalLinkInModel
            {
                public string name { get; set; }
                public string url { get; set; }
            }

            public class EstimateResponse
            {
                public int id { get; set; }
                public int jobId { get; set; }
                public int projectId { get; set; }
                public string name { get; set; }
                public string jobNumber { get; set; }
                public EstimateStatusModel status { get; set; }
                public string summary { get; set; }
                public DateTime modifiedOn { get; set; }
                public DateTime soldOn { get; set; }
                public int soldBy { get; set; }
                public List<EstimateItemResponse> items { get; set; }
                public List<ExternalLinkResponse> externalLinks { get; set; }
            }

            public class EstimateStatusModel
            {
                public int value { get; set; }
                public string name { get; set; }
            }

            public class EstimateItemResponse
            {
                public int id { get; set; }
                public SkuModel sku { get; set; }
                public string skuAccount { get; set; }
                public string description { get; set; }
                public decimal qty { get; set; }
                public decimal unitRate { get; set; }
                public decimal total { get; set; }
                public string itemGroupName { get; set; }
                public int itemGroupRootId { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class SkuModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public string displayName { get; set; }
                public string type { get; set; }
                public decimal soldHours { get; set; }
                public int generalLedgerAccountId { get; set; }
                public string generalLedgerAccountName { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class ExternalLinkResponse
            {
                public string name { get; set; }
                public string url { get; set; }
            }

            public class EstimateItemResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<EstimateItemResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class EstimateResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<EstimateResponse> data { get; set; }
                public int totalCount { get; set; }
            }



            public class SellRequest
            {
                public int soldBy { get; set; }
            }

            public class UpdateEstimateRequest
            {
                public string name { get; set; }
                public string summary { get; set; }
                public int tax { get; set; }
                public List<EstimateItemCreateUpdateRequest> items { get; set; }
                public List<ExternalLinkInModel> externalLinks { get; set; }
            }
        }
        public class Settings
        {
            public class BusinessUnitResponse
            {
                public int id { get; set; }
                public bool active { get; set; }
                public string name { get; set; }
                public string officialName { get; set; }
                public string email { get; set; }
                public string currency { get; set; }
                public string phoneNumber { get; set; }
                public string invoiceHeader { get; set; }
                public string invoiceMessage { get; set; }
                public decimal defaultTaxRate { get; set; }
                public string authorizationParagraph { get; set; }
                public string acknowledgementParagraph { get; set; }
                public BusinessUnitAddressResponse address { get; set; }
                public string materialSku { get; set; }
                public string quickbooksClass { get; set; }
                public string accountCode { get; set; }
                public string franchiseId { get; set; }
                public string conceptCode { get; set; }
                public string corporateContractNumber { get; set; }
                public BusinessUnitTenantResponse tenant { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class BusinessUnitAddressResponse
            {
                public string street { get; set; }
                public string unit { get; set; }
                public string city { get; set; }
                public string state { get; set; }
                public string zip { get; set; }
                public string country { get; set; }
            }

            public class BusinessUnitTenantResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public string quickbooksClass { get; set; }
                public string accountCode { get; set; }
                public string franchiseId { get; set; }
                public string conceptCode { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class BusinessUnitResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<BusinessUnitResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class EmployeeResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public string role { get; set; }
                public int businessUnitId { get; set; }
                public DateTime modifiedOn { get; set; }
                public string email { get; set; }
                public string phoneNumber { get; set; }
                public string loginName { get; set; }
                public List<EmployeeCustomFieldResponse> customFields { get; set; }
                public bool active { get; set; }
            }


            public class EmployeeCustomFieldResponse
            {
                public int typeId { get; set; }
                public string name { get; set; }
                public string value { get; set; }
            }
            public class EmployeeResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<EmployeeResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class TagTypeResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<TagTypeResponse> data { get; set; }
            }

            public class TagTypeResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
            }

            public class TechnicianResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public List<TechnicianResponse> data { get; set; }
                public int totalCount { get; set; }
            }

            public class TechnicianResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public int businessUnitId { get; set; }
                public DateTime modifiedOn { get; set; }
                public string email { get; set; }
                public string phoneNumber { get; set; }
                public string loginName { get; set; }
                public TechnicianAddressResponse home { get; set; }
                public decimal dailyGoal { get; set; }
                public bool isManagedTech { get; set; }
                public List<TechnicianCustomFieldResponse> customFields { get; set; }
                public bool active { get; set; }
            }

            public class TechnicianAddressResponse
            {
                public string street { get; set; }
                public string unit { get; set; }
                public string country { get; set; }
                public string city { get; set; }
                public string state { get; set; }
                public string zip { get; set; }
                public string streetAddress { get; set; }
                public decimal latitude { get; set; }
                public decimal longitude { get; set; }
            }

            public class TechnicianCustomFieldResponse
            {
                public int typeId { get; set; }
                public string name { get; set; }
                public string value { get; set; }
            }
        }
        public class TaskManagement
        {
            public class ClientSideDataResponse
            {
                public List<ClientSideEmployeeResponse> employees { get; set; }
                public List<ClientSideBusinessUnitResponse> businessUnits { get; set; }
                public List<ClientSideTaskPriorityResponse> taskPriorities { get; set; }
                public List<ClientSideTaskResolutionTypeResponse> taskResolutionTypes { get; set; }
                public List<ClientSideTaskStatusResponse> taskStatuses { get; set; }
                public List<ClientSideTaskTypeResponse> taskTypes { get; set; }
                public List<ClientSideTaskSourceResponse> taskSources { get; set; }
                public List<ClientSideTaskResolutionResponse> taskResolutions { get; set; }
            }

            public class ClientSideEmployeeResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
            }

            public class ClientSideBusinessUnitResponse
            {
                public string name { get; set; }
                public int value { get; set; }
            }

            public class ClientSideTaskPriorityResponse
            {
                public string name { get; set; }
            }

            public class ClientSideTaskResolutionTypeResponse
            {
                public string name { get; set; }
            }

            public class ClientSideTaskStatusResponse
            {
                public string name { get; set; }
            }

            public class ClientSideTaskTypeResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
                public List<int> excludedTaskResolutionIds { get; set; }
            }

            public class ClientSideTaskSourceResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
            }

            public class ClientSideTaskResolutionResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public string type { get; set; }
                public bool active { get; set; }
                public List<int> excludedTaskTypeIds { get; set; }
            }

            public class TaskCreateRequest
            {
                public int reportedById { get; set; }
                public int assignedToId { get; set; }
                public bool isClosed { get; set; }
                public string name { get; set; }
                public int businessUnitId { get; set; }
                public int employeeTaskTypeId { get; set; }
                public int employeeTaskSourceId { get; set; }
                public int employeeTaskResolutionId { get; set; }
                public string reportedDate { get; set; }
                public string completeBy { get; set; }
                public List<int> involvedEmployeeIdList { get; set; }
                public int customerId { get; set; }
                public int jobId { get; set; }
                public string description { get; set; }
                public string priority { get; set; }
            }

            public class SubtaskCreateRequest
            {
                public bool isClosed { get; set; }
                public string name { get; set; }
                public int assignedToId { get; set; }
                public string dueDateTime { get; set; }
            }

            public class SubTaskCreateResponse
            {
                public bool isClosed { get; set; }
                public string name { get; set; }
                public int assignedToId { get; set; }
                public DateTime dueDateTime { get; set; }
                public int parentTaskId { get; set; }
                public string subtaskNumber { get; set; }
                public bool isViewed { get; set; }
                public DateTime assignedDateTime { get; set; }
                public DateTime createdOn { get; set; }
            }

            public class TaskCreateResponse
            {
                public int reportedById { get; set; }
                public int assignedToId { get; set; }
                public bool isClosed { get; set; }
                public string name { get; set; }
                public int businessUnitId { get; set; }
                public int employeeTaskTypeId { get; set; }
                public int employeeTaskSourceId { get; set; }
                public int employeeTaskResolutionId { get; set; }
                public DateTime reportedDate { get; set; }
                public DateTime completeBy { get; set; }
                public List<int> involvedEmployeeIdList { get; set; }
                public int customerId { get; set; }
                public int jobId { get; set; }
                public string description { get; set; }
                public string priority { get; set; }
                public int id { get; set; }
                public int taskNumber { get; set; }
                public string customerName { get; set; }
                public string jobNumber { get; set; }
                public int refundIssued { get; set; }
                public DateTime descriptionModifiedOn { get; set; }
                public string descriptionModifiedBy { get; set; }
            }
        }
        public class Telecom
        {
            public class CallInModel
            {
                public int callId { get; set; }
                public DateTime createdOn { get; set; }
                public string duration { get; set; }
                public CallDirection direction { get; set; }
                public CallStatus status { get; set; }
                public string callType { get; set; }
                public string excuseMemo { get; set; }
                public int campaignId { get; set; }
                public int jobId { get; set; }
                public int agentId { get; set; }
                public string recordingUrl { get; set; }
                public string recordingId { get; set; }
                public ReasonInModel reason { get; set; }
                public CustomerInModel customer { get; set; }
                public LocationInModel location { get; set; }
                public string sid { get; set; }
                public DateTime from { get; set; }
                [JsonProperty("to")]
                public DateTime _to { get; set; }
                public string callService { get; set; }
            }

            public class CallDirection
            {
                public string direction { get; set; }
            }

            public class CallStatus
            {
                public string status { get; set; }
            }


            public class ReasonInModel
            {
                public string name { get; set; }
                public bool lead { get; set; }
            }

            public class CustomerInModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public AddressInput address { get; set; }
                public List<ContactInputModel> contacts { get; set; }
            }

            public class AddressInput
            {
                public string street { get; set; }
                public string unit { get; set; }
                public string country { get; set; }
                public string city { get; set; }
                public string state { get; set; }
                public string zip { get; set; }
                public decimal latitude { get; set; }
                public decimal longitude { get; set; }
            }

            public class ContactInputModel
            {
                public int id { get; set; }
                public string type { get; set; }
                public string value { get; set; }
                public string memo { get; set; }
            }

            public class LocationInModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public AddressInput address { get; set; }
                public List<ContactInputModel> contacts { get; set; }
            }

            public class DetailedCallModel
            {
                public int id { get; set; }
                public DateTime receivedOn { get; set; }
                public string duration { get; set; }
                public string from { get; set; }
                [JsonProperty("to")]
                public string _to { get; set; }
                public string direction { get; set; }
                public string callType { get; set; }
                public CallReasonModel reason { get; set; }
                public string recordingUrl { get; set; }
                public string voiceMailUrl { get; set; }
                public NamedModel createdBy { get; set; }
                public CustomerModel customer { get; set; }
                public CampaignModel campaign { get; set; }
                public DateTime modifiedOn { get; set; }
                public CallAgentModel agent { get; set; }
            }



            public class CallReasonResponse_P
            {
                public int page { get; set; }
                public int pageSize { get; set; }
                public bool hasMore { get; set; }
                public int totalCount { get; set; }
                public List<CallReasonResponse> data { get; set; }
            }

            public class CallReasonResponse
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool isLead { get; set; }
                public bool active { get; set; }
            }

            public class CallReasonModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool lead { get; set; }
                public bool active { get; set; }
            }

            public class NamedModel
            {
                public int id { get; set; }
                public string name { get; set; }
            }

            public class CustomerModel
            {
                public int id { get; set; }
                public bool active { get; set; }
                public string name { get; set; }
                public string email { get; set; }
                public int balance { get; set; }
                public bool doNotMail { get; set; }
                public AddressOutput address { get; set; }
                public string importId { get; set; }
                public bool doNotService { get; set; }
                public string type { get; set; }
                public List<ContactOutputModel> contacts { get; set; }
                public int mergedToId { get; set; }
                public DateTime modifiedOn { get; set; }
                public List<MembershipModel> memberships { get; set; }
                public bool hasActiveMembership { get; set; }
                public List<CustomFieldApiModel> customFields { get; set; }
                public DateTime createdOn { get; set; }
                public int createdBy { get; set; }
                public List<CustomerPhoneModel> phoneSettings { get; set; }
            }

            public class AddressOutput
            {
                public string street { get; set; }
                public string unit { get; set; }
                public string country { get; set; }
                public string city { get; set; }
                public string state { get; set; }
                public string zip { get; set; }
                public string streetAddress { get; set; }
                public decimal latitude { get; set; }
                public decimal longitude { get; set; }
            }

            public class ContactOutputModel
            {
                public int id { get; set; }
                public string type { get; set; }
                public string value { get; set; }
                public string memo { get; set; }
                public bool active { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class MembershipModel
            {
                public int id { get; set; }
                public bool active { get; set; }
                public MembershipTypeModel type { get; set; }
                public string status { get; set; }
                public DateTime from { get; set; }
                [JsonProperty("to")]
                public DateTime _to { get; set; }
                public int locationId { get; set; }
            }

            public class MembershipTypeModel
            {
                public int id { get; set; }
                public bool active { get; set; }
                public string name { get; set; }
            }

            public class CustomFieldApiModel
            {
                public int typeId { get; set; }
                public string name { get; set; }
                public string value { get; set; }
            }

            public class CustomerPhoneModel
            {
                public string phoneNumber { get; set; }
                public bool doNotText { get; set; }
            }

            public class CampaignModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public DateTime modifiedOn { get; set; }
                public bool active { get; set; }
                public CampaignCategoryModel category { get; set; }
            }

            public class CampaignCategoryModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public bool active { get; set; }
            }

            public class CallAgentModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public int externalId { get; set; }
            }

            public class BundleCallModel_P
            {
                public List<BundleCallModel> data { get; set; }
                public int page { get; set; }
                public int pageSize { get; set; }
                public int totalCount { get; set; }
                public bool hasMore { get; set; }
            }

            public class BundleCallModel
            {
                public int id { get; set; }
                public string jobNumber { get; set; }
                public int projectId { get; set; }
                public BusinessUnitModel businessUnit { get; set; }
                public JobTypeModel type { get; set; }
                public CallModel leadCall { get; set; }
            }

            public class BusinessUnitModel
            {
                public int id { get; set; }
                public bool active { get; set; }
                public string name { get; set; }
                public string officialName { get; set; }
                public string email { get; set; }
                public string currency { get; set; }
                public string phoneNumber { get; set; }
                public string invoiceHeader { get; set; }
                public string invoiceMessage { get; set; }
                public decimal defaultTaxRate { get; set; }
                public string authorizationParagraph { get; set; }
                public string acknowledgementParagraph { get; set; }
                public BusinessUnitAddressModel address { get; set; }
                public string materialSku { get; set; }
                public string quickbooksClass { get; set; }
                public string accountCode { get; set; }
                public string franchiseId { get; set; }
                public string conceptCode { get; set; }
                public string corporateContractNumber { get; set; }
                public BusinessUnitTenantModel tenant { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class BusinessUnitAddressModel
            {
                public string street { get; set; }
                public string unit { get; set; }
                public string city { get; set; }
                public string state { get; set; }
                public string zip { get; set; }
                public string country { get; set; }
            }

            public class BusinessUnitTenantModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public string quickbooksClass { get; set; }
                public string accountCode { get; set; }
                public string franchiseId { get; set; }
                public string conceptCode { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class JobTypeModel
            {
                public int id { get; set; }
                public string name { get; set; }
                public DateTime modifiedOn { get; set; }
            }

            public class CallModel
            {
                public int id { get; set; }
                public DateTime receivedOn { get; set; }
                public string duration { get; set; }
                public string from { get; set; }
                [JsonProperty("to")]
                public string _to { get; set; }
                public string direction { get; set; }
                public string callType { get; set; }
                public CallReasonModel reason { get; set; }
                public string recordingUrl { get; set; }
                public string voiceMailUrl { get; set; }
                public NamedModel createdBy { get; set; }
                public CustomerModel customer { get; set; }
                public CampaignModel campaign { get; set; }
                public DateTime modifiedOn { get; set; }
                public CallAgentModel agent { get; set; }
            }

            public class DetailedBundleCallModel
            {
                public int id { get; set; }
                public string jobNumber { get; set; }
                public int projectId { get; set; }
                public BusinessUnitModel businessUnit { get; set; }
                public JobTypeModel type { get; set; }
                public DetailedCallModel leadCall { get; set; }
            }

            public class CallInUpdateModelV2
            {
                public int callId { get; set; }
                public string callType;
                public string excuseMemo;
                public int? campaignId;
                public int? jobId;
                public ReasonInModel reason { get; set; }
                public CustomerInModel customer { get; set; }
                public LocationInModel location { get; set; }
            }
        }
    }
    public class Functions : Internal
    {
        public class Accounting : Types.Accounting
        {
            /// <summary>
            ///             ''' Creates a new adjustment invoice to an already existing invoice.
            ///             ''' </summary>
            ///             ''' <param name="payload">An instance of 'AdjustmentInvoiceCreateRequest'</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
            public static object createAdjustmentInvoices(Types.Accounting.AdjustmentInvoiceCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/invoices";



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Deletes a specific invoice item from a specific invoice.
            ///             ''' </summary>
            ///             ''' <param name="invoiceId">Invoice ID#</param>
            ///             ''' <param name="invoiceItemId">Invoice Line Item ID#</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
            public static object deleteInvoiceItem(int invoiceId, int invoiceItemId, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/invoices/" + invoiceId + "/items/" + invoiceItemId;



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a full list of invoices matching your specified filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">Your options, if you desire to filter the results (If applicable)</param>
            ///             ''' <returns>Returns either a paginated list of invoices (InvoiceResponse_P) or a ServiceTitanError class.</returns>
            public static object getInvoiceList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/invoices";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Accounting.InvoiceResponse_P results = JsonConvert.DeserializeObject<Types.Accounting.InvoiceResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            // TODO: markAsExported: Bugged. Needs to be fixed properly.
            /// <summary>
            ///             ''' markAsExported: Endpoint is bugged (Does not allow specifying an invoiceid). Do not use!
            ///             ''' </summary>
            ///             ''' <param name="invoiceId"></param>
            ///             ''' <param name="invoiceItemId"></param>
            ///             ''' <param name="accesstoken"></param>
            ///             ''' <param name="STAppKey"></param>
            ///             ''' <param name="tenant"></param>
            ///             ''' <returns></returns>
            public static object markAsExported(int invoiceId, int invoiceItemId, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/invoices/" + invoiceId + "/items/" + invoiceItemId;



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an invoice's information.
            ///             ''' </summary>
            ///             ''' <param name="invoiceId">Invoice ID#</param>
            ///             ''' <param name="invoice">An instance of InvoiceUpdateRequest</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
            public static object UpdateInvoice(int invoiceId, Types.Accounting.InvoiceUpdateRequest invoice, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/invoices/" + invoiceId;



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(invoice));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates invoice items.
            ///             ''' </summary>
            ///             ''' <param name="invoiceId">Invoice ID#</param>
            ///             ''' <param name="invoice">Invoice Items Payload (As InvoiceUpdateRequest)</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
            public static object UpdateInvoiceItems(int invoiceId, Types.Accounting.InvoiceItemUpdateRequest invoice, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/invoices/" + invoiceId + "/items";



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(invoice));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates custom fields in one or many invoices.
            ///             ''' </summary>
            ///             ''' <param name="customfields">Contains an array of invoices, with an array of custom fields.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
            public static object UpdateInvoiceCustomFields(Types.Accounting.CustomFieldUpdateRequest customfields, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/invoices/custom-fields";



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(customfields));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Adds a payment to an existing invoice.
            ///             ''' </summary>
            ///             ''' <param name="payment">PaymentCreateRequest payload definiting what payment to add, and where.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>The new payment successfully created (As PaymentResponse), or a ServiceTitanError.</returns>
            public static object createPayment(Types.Accounting.PaymentCreateRequest payment, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/payments";



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payment));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();

                    streamread.Close();
                    buffer.Close();
                    response.Close();

                    Types.Accounting.PaymentResponse deserialized = JsonConvert.DeserializeObject<Types.Accounting.PaymentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    return deserialized;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates custom fields in one or many payments.
            ///             ''' </summary>
            ///             ''' <param name="customfields">Contains an array of invoices, with an array of custom fields.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
            public static object UpdatePaymentCustomFields(Types.Accounting.CustomFieldUpdateRequest customfields, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/payments/custom-fields";



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(customfields));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates one or many payments' statuses to one of the following: Pending, Posted or Exported.
            ///             ''' </summary>
            ///             ''' <param name="payment">The payment status change request(s)</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
            public static object updatePaymentStatus(Types.Accounting.PaymentStatusBatchRequest payment, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/payments/status";



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payment));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }

                // Dim deserialized As PaymentResponse = JsonConvert.DeserializeObject(Of PaymentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                // Return deserialized
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a single payment.
            ///             ''' </summary>
            ///             ''' <param name="payment">Contains the payload to update the payment.</param>
            ///             ''' <param name="paymentid">The payment unique ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>The new payment successfully updated (As PaymentResponse), or a ServiceTitanError.</returns>
            public static object updatePayment(Types.Accounting.PaymentUpdateRequest payment, int paymentid, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/payments/" + paymentid;



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payment));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();

                    streamread.Close();
                    buffer.Close();
                    response.Close();

                    Types.Accounting.PaymentResponse deserialized = JsonConvert.DeserializeObject<Types.Accounting.PaymentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    return deserialized;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            // TODO: Implement pagination once it is implemented API-side
            /// <summary>
            ///             ''' Gets a list of payments depending on filters set. This API currently has arguments for pagination, but the json output has no support for it. So pagination has to be guessed.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters or preferences for this function</param>
            ///             ''' <returns>Either returns a (unpaginated) list of payments, or a ServiceTitanError.</returns>
            public static object getPaymentsList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/payments";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    List<Types.Accounting.DetailedPaymentResponse> results = JsonConvert.DeserializeObject<List<Types.Accounting.DetailedPaymentResponse>>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets the current default payment terms for the specified customer.
            ///             ''' </summary>
            ///             ''' <param name="customerid">The requested customer's ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either an instance of PaymentTermModel, or a ServiceTitanError.</returns>
            public static object getCustomerDefaultPaymentTerm(int customerid, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/payment-terms/" + customerid;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    List<Types.Accounting.PaymentTermModel> results = JsonConvert.DeserializeObject<List<Types.Accounting.PaymentTermModel>>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a paginated list of payment types. (Note: Does not fetch any subsequent pages, use additional options to go through them)
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters or preferences for this function</param>
            ///             ''' <returns>Returns either a paginated list of payment types (PaymentTypeResponse_P) or a ServiceTitanError.</returns>
            public static object getPaymentTypesList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/payment-types";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Accounting.PaymentTypeResponse_P results = JsonConvert.DeserializeObject<Types.Accounting.PaymentTypeResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single payment type, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="PaymentTypeID">The payment type ID.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a payment type (PaymentTypeResponse) or a ServiceTitanError.</returns>
            public static object getPaymentType(int PaymentTypeID, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/payment-types/" + PaymentTypeID;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Accounting.PaymentTypeResponse results = JsonConvert.DeserializeObject<Types.Accounting.PaymentTypeResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Returns a paginated list of tax zones. Does not get other pages, use pagination to cycle through them.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters or preferences for this function</param>
            ///             ''' <returns>Returns either a paginated list of tax zones (TaxZoneResponse_P) or a ServiceTitanError.</returns>
            public static object getTaxZonesList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/accounting/v2/tenant/" + tenant + "/tax-zones";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Accounting.TaxZoneResponse_P results = JsonConvert.DeserializeObject<Types.Accounting.TaxZoneResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class CRM : Types.CRM
        {
            // UPDATED: 1/17/2022
            /// <summary>
            ///             ''' Gets a full list of customer contacts, based on the id.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="id">The customer ID</param>
            ///             ''' <returns>Returns either a paginated list of Customer Contacts (CustomerContactWithModifiedOnResponse_P) or a ServiceTitanError.</returns>
            public static object getCustomerContacts(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, int id, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/customers/" + id + "/contacts";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.CustomerContactWithModifiedOnResponse_P results = JsonConvert.DeserializeObject<Types.CRM.CustomerContactWithModifiedOnResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a full list of customers, based on your filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of OptionList</param>
            ///             ''' <returns>Returns either a paginated list of Customers (CustomerResponse_P) or a ServiceTitanError.</returns>
            public static object getCustomerList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/customers";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.CustomerResponse_P results = JsonConvert.DeserializeObject<Types.CRM.CustomerResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single customer, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="customerid">The customer ID to obtain</param>
            ///             ''' <returns>Returns either a paginated list of Customers (CustomerResponse_P) or a ServiceTitanError.</returns>
            public static object getCustomer(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, int customerid)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/customers/" + customerid;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.CustomerResponse results = JsonConvert.DeserializeObject<Types.CRM.CustomerResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new customer record.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="payload">The customer information to add to the new file.</param>
            ///             ''' <returns>Returns an instance of CreatedCustomerResponse or a ServiceTitanError.</returns>
            public static object createCustomer(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, Types.CRM.CreateCustomerRequest payload)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/customers";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.CreatedCustomerResponse results = JsonConvert.DeserializeObject<Types.CRM.CreatedCustomerResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a customer file.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="customerid">The customer ID to update</param>
            ///             ''' <param name="payload">The data to update the customer with.</param>
            ///             ''' <returns>Either returns a CustomerResponse, or a ServiceTitanError.</returns>
            public static object updateCustomer(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, int customerid, Types.CRM.UpdateCustomerRequest payload)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/customers/" + customerid;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.CustomerResponse results = JsonConvert.DeserializeObject<Types.CRM.CustomerResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a full list of locations, based on your filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of OptionList</param>
            ///             ''' <returns>Returns either a paginated list of locations (LocationResponse_P) or a ServiceTitanError.</returns>
            public static object getLocationList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/locations";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.LocationResponse_P results = JsonConvert.DeserializeObject<Types.CRM.LocationResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single location, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="locationid">The location ID to obtain</param>
            ///             ''' <returns>Returns either a paginated list of locations (LocationResponse_P) or a ServiceTitanError.</returns>
            public static object getLocation(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, int locationid)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/locations/" + locationid;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.LocationResponse results = JsonConvert.DeserializeObject<Types.CRM.LocationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new location record.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="payload">The location information to add to the new file.</param>
            ///             ''' <returns>Returns an instance of CreatedLocationResponse or a ServiceTitanError.</returns>
            public static object createLocation(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, Types.CRM.CreateLocationRequest payload)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/locations";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.CreatedCustomerResponse results = JsonConvert.DeserializeObject<Types.CRM.CreatedCustomerResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a location file.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="locationid">The location ID to update</param>
            ///             ''' <param name="payload">The data to update the customer with.</param>
            ///             ''' <returns>Either returns a LocationResponse, or a ServiceTitanError.</returns>
            public static object updateLocation(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, int locationid, Types.CRM.UpdateLocationRequest payload)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/locations/" + locationid;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.LocationResponse results = JsonConvert.DeserializeObject<Types.CRM.LocationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }

            // END OF UPDATE


            /// <summary>
            ///             ''' Gets a specific lead by it's internal ID.
            ///             ''' </summary>
            ///             ''' <param name="leadid">The lead ID number</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a single lead (LeadResponse) or a ServiceTitanError.</returns>
            public static object getLead(int leadid, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/leads";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.LeadResponse results = JsonConvert.DeserializeObject<Types.CRM.LeadResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Returns a paginated list of leads. Does not get other pages, use pagination to cycle through them.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters or preferences for this function</param>
            ///             ''' <returns>Returns either a paginated list of Leads (LeadResponse_P) or a ServiceTitanError.</returns>
            public static object getLeadList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/leads";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.LeadResponse_P results = JsonConvert.DeserializeObject<Types.CRM.LeadResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Returns a paginated list of tags. Does not get other pages, use pagination to cycle through them.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters or preferences for this function</param>
            ///             ''' <returns>Returns either a paginated list of tags (TagsResponse_P) or a ServiceTitanError.</returns>
            public static object getTagList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/crm/v2/tenant/" + tenant + "/tags";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.CRM.TagsResponse_P results = JsonConvert.DeserializeObject<Types.CRM.TagsResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class Dispatch : Types.Dispatch
        {
            // UPDATED: 1/18/22

            /// <summary>
            ///             ''' Gets a full list of Non-job Appointments, based on your filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of OptionList</param>
            ///             ''' <returns>Returns either a paginated list of Non-job appointments (NonJobAppointmentResponse_P) or a ServiceTitanError.</returns>
            public static object getNonJobAppointmentList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/dispatch/v2/tenant/" + tenant + "/non-job-appointments";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Dispatch.NonJobAppointmentResponse_P results = JsonConvert.DeserializeObject<Types.Dispatch.NonJobAppointmentResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single non-job appointment, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="njaid">The non-job appointment ID to obtain</param>
            ///             ''' <returns>Returns either a non-job appointment (LocationResponse) or a ServiceTitanError.</returns>
            public static object getNonJobAppointment(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, int njaid)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/dispatch/v2/tenant/" + tenant + "/non-job-appointments/" + njaid;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Dispatch.NonJobAppointmentResponse results = JsonConvert.DeserializeObject<Types.Dispatch.NonJobAppointmentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new non-job appointment record.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="payload">The non-job appointment information to add to the new file.</param>
            ///             ''' <returns>Returns an instance of NonJobAppointmentResponse or a ServiceTitanError.</returns>
            public static object createNonJobAppointment(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, Types.Dispatch.NonJobAppointmentCreateRequest payload)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/dispatch/v2/tenant/" + tenant + "/non-job-appointments";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Dispatch.NonJobAppointmentResponse results = JsonConvert.DeserializeObject<Types.Dispatch.NonJobAppointmentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a non-job appointment file.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="njaid">The appointment ID to update</param>
            ///             ''' <param name="payload">The data to update the appointment with.</param>
            ///             ''' <returns>Either returns a NonJobAppointmentResponse, or a ServiceTitanError.</returns>
            public static object updateNonJobAppointment(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, int njaid, Types.Dispatch.NonJobAppointmentUpdateRequest payload)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/dispatch/v2/tenant/" + tenant + "/non-job-appointments/" + njaid;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Dispatch.NonJobAppointmentResponse results = JsonConvert.DeserializeObject<Types.Dispatch.NonJobAppointmentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }


            // END OF UPDATE

            /// <summary>
            ///             ''' Returns a paginated list of appointment assignments. Does not get other pages, use pagination to cycle through them.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters or preferences for this function</param>
            ///             ''' <returns>Returns either a paginated list of appointment assignments (AppointmentAssignmentResponse_P) or a ServiceTitanError.</returns>
            public static object getAppointmentAssignmentList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/dispatch/v2/tenant/" + tenant + "/appointment-assignments";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Dispatch.AppointmentAssignmentResponse_P results = JsonConvert.DeserializeObject<Types.Dispatch.AppointmentAssignmentResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets the availability for the requested timeframe using the information provided in the CapacityQueryFilter.
            ///             ''' </summary>
            ///             ''' <param name="payload">Your request parameters for the filter.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either the current availabilities (CapacityResponse) or a ServiceTitanError.</returns>
            public static object getCapacity(Types.Dispatch.CapacityQueryFilter payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/dispatch/v2/tenant/" + tenant + "/capacity";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Dispatch.CapacityResponse results = JsonConvert.DeserializeObject<Types.Dispatch.CapacityResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Returns a paginated list of technician shifts. Does not get other pages, use pagination to cycle through them.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters or preferences for this function</param>
            ///             ''' <returns>Returns either a paginated list of technician shifts (TechnicianShiftResponse_P) or a ServiceTitanError.</returns>
            public static object getTechnicianShifts(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/dispatch/v2/tenant/" + tenant + "/technician-shifts";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Dispatch.TechnicianShiftResponse_P results = JsonConvert.DeserializeObject<Types.Dispatch.TechnicianShiftResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single technician shift, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The unique id of the shift record</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a technician shifts (TechnicianShiftResponse) or a ServiceTitanError.</returns>
            public static object getTechnicianShift(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/dispatch/v2/tenant/" + tenant + "/technician-shifts/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Dispatch.TechnicianShiftResponse results = JsonConvert.DeserializeObject<Types.Dispatch.TechnicianShiftResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class EquipmentSystems : Types.EquipmentSystems
        {
            /// <summary>
            ///             ''' Creates a new Installed Equipment on a location ID.
            ///             ''' </summary>
            ///             ''' <param name="equipment">Information to add the new equipment to the file.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns an instance of the new installed equipment (InstalledEquipmentDetailedResponse), or a ServiceTitanError.</returns>
            public static object createInstalledEquipment(Types.EquipmentSystems.InstalledEquipmentCreateRequest equipment, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/equipmentsystems/v2/tenant/" + tenant + "/installed-equipment";



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(equipment));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();

                    streamread.Close();
                    buffer.Close();
                    response.Close();

                    Types.EquipmentSystems.InstalledEquipmentDetailedResponse deserialized = JsonConvert.DeserializeObject<Types.EquipmentSystems.InstalledEquipmentDetailedResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    return deserialized;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Returns a single installed equipment by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The installed equipment ID.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either an installed equipment (InstalledEquipmentDetailedResponse) or a ServiceTitanError.</returns>
            public static object getInstalledEquipment(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/equipmentsystems/v2/tenant/" + tenant + "/installed-equipment/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.EquipmentSystems.InstalledEquipmentDetailedResponse results = JsonConvert.DeserializeObject<Types.EquipmentSystems.InstalledEquipmentDetailedResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Returns a paginated list of Installed Equipments.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters to limit the list of returned records</param>
            ///             ''' <returns>Returns either a paginated list of installed equipments (InstalledEquipmentResponse_P) or a ServiceTitanError.</returns>
            public static object getInstalledEquipments(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/equipmentsystems/v2/tenant/" + tenant + "/installed-equipment";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.EquipmentSystems.InstalledEquipmentResponse_P results = JsonConvert.DeserializeObject<Types.EquipmentSystems.InstalledEquipmentResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an existing installed equipment record, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The installed equipment ID</param>
            ///             ''' <param name="equipment">The information to update on the equipment.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, the request returns the updated equipment (InstalledEquipmentDetailedResponse) or a ServiceTitanError.</returns>
            public static object UpdateInstalledEquipment(int id, Types.EquipmentSystems.InstalledEquipmentUpdateRequest equipment, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/equipmentsystems/v2/tenant/" + tenant + "/installed-equipment/" + id;



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(equipment));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    Types.EquipmentSystems.InstalledEquipmentDetailedResponse results = JsonConvert.DeserializeObject<Types.EquipmentSystems.InstalledEquipmentDetailedResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class Forms : Types.Forms {
            /// <summary>
            ///             ''' Gets a full list of form submissions matching your specified filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">Your options, if you desire to filter the results (If applicable)</param>
            ///             ''' <returns>Returns either a paginated list of forms (FormSubmissionResponse_P) or a ServiceTitanError class.</returns>
            public static object getFormSubmissionList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/forms/v2/tenant/" + tenant + "/submissions";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/x-www-form-urlencoded";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Console.WriteLine("Received: " + output);
                    Types.Forms.FormSubmissionResponse_P results = JsonConvert.DeserializeObject<Types.Forms.FormSubmissionResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class Inventory : Types.Inventory
        {
            /// <summary>
            ///             ''' Creates a new purchase order, using a CreatePurchaseOrderRequest payload.
            ///             ''' </summary>
            ///             ''' <param name="purchaseorder">The new purchase order to create</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, the request returns the created equipment's ID as a CreatePurchaseOrderResponse class, or a ServiceTitanError.</returns>
            public static object createPurchaseOrder(Types.Inventory.CreatePurchaseOrderRequest purchaseorder, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/inventory/v2/tenant/" + tenant + "/purchase-orders";



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(purchaseorder));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();

                    streamread.Close();
                    buffer.Close();
                    response.Close();

                    Types.Inventory.CreatePurchaseOrderResponse deserialized = JsonConvert.DeserializeObject<Types.Inventory.CreatePurchaseOrderResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    return deserialized;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Queries for a list of vendors, using a list of OptionsList as filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters, as well as options to cycle through pages.</param>
            ///             ''' <returns>A paginated list of vendors (as VendorResponse_P), or a ServiceTitanError.</returns>
            public static object getVendorList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/inventory/v2/tenant/" + tenant + "/vendors";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Console.WriteLine("API Output: " + output);
                    Types.Inventory.VendorResponse_P results = JsonConvert.DeserializeObject<Types.Inventory.VendorResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Queries for a list of purchase orders, using a list of OptionsList as filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters, as well as options to cycle through pages.</param>
            ///             ''' <returns>A paginated list of purchase orders (as PurchaseOrderResponse_P), or a ServiceTitanError.</returns>
            public static object getPurchaseOrders(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/inventory/v2/tenant/" + tenant + "/purchase-orders";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Console.WriteLine("API Output: " + output);
                    Types.Inventory.PurchaseOrderResponse_P results = JsonConvert.DeserializeObject<Types.Inventory.PurchaseOrderResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Queries for a list of warehouses and trucks, using a list of OptionsList as filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters, as well as options to cycle through pages.</param>
            ///             ''' <returns>A paginated list of warehouses (as WarehouseReponse_P), or a ServiceTitanError.</returns>
            public static object getWarehouses(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/inventory/v2/tenant/" + tenant + "/warehouses";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Console.WriteLine("API Output: " + output);
                    Types.Inventory.WarehouseResponse_P results = JsonConvert.DeserializeObject<Types.Inventory.WarehouseResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single Purchase Order by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">Purchase Order ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns a PurchaseOrderResponse, or a ServiceTitanError.</returns>
            public static object getPurchaseOrder(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/inventory/v2/tenant/" + tenant + "/purchase-orders";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Inventory.PurchaseOrderResponse results = JsonConvert.DeserializeObject<Types.Inventory.PurchaseOrderResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an existing purchase order
            ///             ''' </summary>
            ///             ''' <param name="poId">The Purchase Order ID to update.</param>
            ///             ''' <param name="purchaseorder">The updated purchase order</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns an instance of CreatePurchaseOrderResponse, or a ServiceTitanError</returns>
            public static object UpdatePurchaseOrder(int poId, Types.Inventory.UpdatePurchaseOrderRequest purchaseorder, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/inventory/v2/tenant/" + tenant + "/purchase-orders/" + poId;



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(purchaseorder));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Inventory.CreatePurchaseOrderResponse results = JsonConvert.DeserializeObject<Types.Inventory.CreatePurchaseOrderResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class JobBooking_ContactExperience : Types.JobBooking_ContactExperience
        {
            /// <summary>
            ///             ''' Gets a list of call reasons, depending on the provided OptionsList.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A filter list for this request</param>
            ///             ''' <returns>Returns either a paginated list of CallReasonResponse (CallReasonResponse_P) or a ServiceTitanError.</returns>
            public static object getCallReasons(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jbce/v2/tenant/" + tenant + "/call-reasons";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobBooking_ContactExperience.CallReasonResponse_P results = JsonConvert.DeserializeObject<Types.JobBooking_ContactExperience.CallReasonResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class JobPlanning_Management : Types.JobPlanning_Management
        {
            /// <summary>
            ///             ''' Creates a new appointment onto an existing job.
            ///             ''' </summary>
            ///             ''' <param name="appointment">The new appointment to create</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either the created appointment (AppointmentResponse) or a ServiceTitanError.</returns>
            public static object createAppointment(Types.JobPlanning_Management.AppointmentAddRequest appointment, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/appointments";



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(appointment));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();

                    streamread.Close();
                    buffer.Close();
                    response.Close();

                    Types.JobPlanning_Management.AppointmentResponse deserialized = JsonConvert.DeserializeObject<Types.JobPlanning_Management.AppointmentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    return deserialized;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates the special instructions on an existing appointment.
            ///             ''' </summary>
            ///             ''' <param name="id">The appointment ID</param>
            ///             ''' <param name="instructions">The special instructions</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either the updated appointment (AppointmentResponse) or a ServiceTitanError.</returns>
            public static object updateAppointmentSpecialInstructions(int id, Types.JobPlanning_Management.UpdateAppointmentSpecialInstructionsRequest instructions, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/appointments/" + id + "/special-instructions";



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(instructions));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();

                    streamread.Close();
                    buffer.Close();
                    response.Close();

                    Types.JobPlanning_Management.AppointmentResponse deserialized = JsonConvert.DeserializeObject<Types.JobPlanning_Management.AppointmentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    return deserialized;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Deletes an appointment.
            ///             ''' </summary>
            ///             ''' <param name="id">The appointment's ID.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, this function returns Nothing, or a ServiceTitanError.</returns>
            public static object deleteAppointment(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);
                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxAuthEnvironment;
                    else
                        domain = "https://" + productionAuthEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/appointments/" + id;



                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;

                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(instructions))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();

                    streamread.Close();
                    buffer.Close();
                    response.Close();

                    // Dim deserialized As AppointmentResponse = JsonConvert.DeserializeObject(Of AppointmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a paginated list of appointments, based on the specified OptionsList.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">The options list for this parameter (Filters)</param>
            ///             ''' <returns>Returns either a paginated list of AppointmentResponse (AppointmentResponse_P) or a ServiceTitanError.</returns>
            public static object getAppointments(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/appointments";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.AppointmentResponse_P results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.AppointmentResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single appointment, by it's ID
            ///             ''' </summary>
            ///             ''' <param name="id">The appointment's ID.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a AppointmentResponse, or a ServiceTitanError.</returns>
            public static object getAppointment(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/appointments/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.AppointmentResponse results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.AppointmentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Puts an appointment on hold.
            ///             ''' </summary>
            ///             ''' <param name="id">The appointment ID</param>
            ///             ''' <param name="reason">The reason for putting the appointment on hold</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, the function returns Nothing. If not, it returns a ServiceTitanError.</returns>
            public static object putAppointmentOnHold(int id, Types.JobPlanning_Management.HoldAppointmentRequest reason, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/appointments/" + id + "/hold";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reason));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As AppointmentResponse_P = JsonConvert.DeserializeObject(Of AppointmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Removes hold from the appointment.
            ///             ''' </summary>
            ///             ''' <param name="id">The appointment ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, the function returns Nothing. If not, it returns a ServiceTitanError.</returns>

            public static object removeAppointmentFromHold(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/appointments/" + id + "/hold";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reason))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As AppointmentResponse_P = JsonConvert.DeserializeObject(Of AppointmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Reschedules a single appointment.
            ///             ''' </summary>
            ///             ''' <param name="id">The appointment's ID.</param>
            ///             ''' <param name="reschedulerequest">The reschedule request</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, returns Nothing. If the request fails, returns a ServiceTitanError.</returns>
            public static object rescheduleAppointment(int id, Types.JobPlanning_Management.AppointmentRescheduleRequest reschedulerequest, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/appointments/" + id + "/reschedule";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reschedulerequest));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As AppointmentResponse_P = JsonConvert.DeserializeObject(Of AppointmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of Job Cancel Reasons, in a paginated format.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/filters for this request</param>
            ///             ''' <returns>Returns either a paginated list of Job Cancel Reasons (JobCancelReasonResponse_P) or a ServiceTitanError.</returns>
            public static object getJobCancelReasonsList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/job-cancel-reasons";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobCancelReasonResponse_P results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobCancelReasonResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of Job on Hold Reasons, in a paginated format.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/filters for this request</param>
            ///             ''' <returns>Returns either a paginated list of Job On Hold Reasons (JobHoldReasonResponse_P) or a ServiceTitanError.</returns>
            public static object getJobHoldReasons(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/job-hold-reasons";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobHoldReasonResponse_P results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobHoldReasonResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Cancels the specified job, for the specified reason.
            ///             ''' </summary>
            ///             ''' <param name="jobid">The Job ID to cancel</param>
            ///             ''' <param name="payload">The payload, containing the reason for cancellation.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, request returns Nothing. If error, returns a ServiceTitanError.</returns>
            public static object cancelJob(int jobid, Types.JobPlanning_Management.CancelJobRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/jobs/" + jobid + "/cancel";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As JobHoldReasonResponse_P = JsonConvert.DeserializeObject(Of JobHoldReasonResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new job
            ///             ''' </summary>
            ///             ''' <param name="payload">The data to put in the new job</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a JobResponse, or a ServiceTitanError.</returns>
            public static object createJob(Types.JobPlanning_Management.JobCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/jobs/";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobResponse results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new note on a job.
            ///             ''' </summary>
            ///             ''' <param name="jobid">The job ID where the note should appear.</param>
            ///             ''' <param name="payload">The note payload</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a NoteResponse, or a ServiceTitanError.</returns>
            public static object createJobNote(int jobid, Types.JobPlanning_Management.JobNoteCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/jobs/" + jobid + "/notes";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.NoteResponse results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.NoteResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single job, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="jobid">The job ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of OptionList. Right now, the only option available for this API endpoint is externalDataApplicationGuid.</param>
            ///             ''' <returns>Either returns a JobResponse, or a ServiceTitanError.</returns>
            public static object getJob(int jobid, oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/jobs/" + jobid;

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobResponse results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of Cancel Reasons for a job.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/filters.</param>
            ///             ''' <returns>Either returns a paginated list of Cancel reasons (CancelReasonResponse_P) or a ServiceTitanError.</returns>
            public static object getJobCancelReasons(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/jobs/cancel-reasons";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.CancelReasonResponse_P results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.CancelReasonResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Returns a job's full history.
            ///             ''' </summary>
            ///             ''' <param name="jobid">The job ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a JobHistoryResponse, or a ServiceTitanError.</returns>
            public static object getJobHistory(int jobid, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/jobs/" + jobid + "/history";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobHistoryResponse results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobHistoryResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of jobs (Based on your filters/options)
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/filters for this endpoint.</param>
            ///             ''' <returns>Either returns a paginated list of jobs (JobResponse_P) or a ServiceTitanError.</returns>
            public static object getJobList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/jobs/";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobResponse_P results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a paginated list of a job's notes.
            ///             ''' </summary>
            ///             ''' <param name="jobid">The job's ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/filters for this endpoint.</param>
            ///             ''' <returns>Returns either a paginated list of notes (NoteResponse_P) or a ServiceTitanError.</returns>
            public static object getJobNotes(int jobid, oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/jobs/" + jobid + "/notes";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.NoteResponse_P results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.NoteResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Puts a job on hold. Requires a reason.
            ///             ''' </summary>
            ///             ''' <param name="jobid">Job ID</param>
            ///             ''' <param name="payload">The reason</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns Nothing (if successful) or a ServiceTitanError.</returns>
            public static object putJobOnHold(int jobid, Types.JobPlanning_Management.HoldJobRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/jobs/" + jobid + "/hold";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As NoteResponse_P = JsonConvert.DeserializeObject(Of NoteResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a job with new information.
            ///             ''' </summary>
            ///             ''' <param name="jobid">Job's ID</param>
            ///             ''' <param name="payload">The updated job information.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a JobResponse, or a ServiceTitanError.</returns>
            public static object updateJob(int jobid, Types.JobPlanning_Management.UpdateJobRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/jobs/" + jobid;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobResponse results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a single job type with new information.
            ///             ''' </summary>
            ///             ''' <param name="jobTypeID">The job type's ID.</param>
            ///             ''' <param name="payload">The job type payload</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a JobTypeResponse, or a ServiceTitanError.</returns>
            public static object updateJobType(int jobTypeID, Types.JobPlanning_Management.UpdateJobTypeRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/job-types/" + jobTypeID;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobTypeResponse results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobTypeResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new job type.
            ///             ''' </summary>
            ///             ''' <param name="payload">The job type to create</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a JobTypeResponse, or a ServiceTitanError.</returns>
            public static object createJobType(Types.JobPlanning_Management.CreateJobTypeRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/job-types";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobTypeResponse results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobTypeResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single job type, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="jobTypeID">The job type's ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/filters for this endpoint.</param>
            ///             ''' <returns>Either returns a JobTypeResponse, or a ServiceTitanError.</returns>
            public static object getJobType(int jobTypeID, oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/job-types/" + jobTypeID;

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobTypeResponse results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobTypeResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of job types, based on your options/filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/filters for this endpoint.</param>
            ///             ''' <returns>Either returns a paginated list of job types (JobTypeResponse_P) or a ServiceTitanError.</returns>
            public static object getJobTypeList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/job-types";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.JobTypeResponse_P results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.JobTypeResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of projects, based on your options/filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/filters for this endpoint.</param>
            ///             ''' <returns>Either returns a paginated list of projects (ProjectResponse_P) or a ServiceTitanError.</returns>

            public static object getProjectList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/projects";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.ProjectResponse_P results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.ProjectResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single project, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="projectID">The project's ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a ProjectResponse, or a ServiceTitanError.</returns>
            public static object getProject(int projectID, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/jpm/v2/tenant/" + tenant + "/projects/" + projectID;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.JobPlanning_Management.ProjectResponse results = JsonConvert.DeserializeObject<Types.JobPlanning_Management.ProjectResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class Marketing : Types.Marketing
        {
            /// <summary>
            ///             ''' Creates a new campaign category
            ///             ''' </summary>
            ///             ''' <param name="payload">The category payload</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            public static object createCampaignCategories(Types.Marketing.CampaignCategoryCreateUpdateModel payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/categories";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As ProjectResponse = JsonConvert.DeserializeObject(Of ProjectResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of Campaign Categories (Not paginated).
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a list of Campaigns (List Of(CampaignCategoryModel)) or a ServiceTitanError.</returns>
            public static object getCampaignCategoriesList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/categories";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    List<Types.Marketing.CampaignCategoryModel> results = JsonConvert.DeserializeObject<List<Types.Marketing.CampaignCategoryModel>>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single Campaign Category.
            ///             ''' </summary>
            ///             ''' <param name="id">The Campaign Category ID.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a CampaignCategoryModel, or a ServiceTitanError.</returns>
            public static object getCampaignCategory(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/categories/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Marketing.CampaignCategoryModel results = JsonConvert.DeserializeObject<Types.Marketing.CampaignCategoryModel>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a single campaign category by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The campaign category id.</param>
            ///             ''' <param name="payload">The updated campaign category</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, returns Nothing. Otherwise, returns a ServiceTitanError.</returns>
            public static object updateCampaignCategory(int id, Types.Marketing.CampaignCategoryCreateUpdateModel payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/categories/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As CampaignCategoryModel = JsonConvert.DeserializeObject(Of CampaignCategoryModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new campaign cost record.
            ///             ''' </summary>
            ///             ''' <param name="payload">The new campaign cost</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, returns a CampaignCostModel. Otherwise, returns a ServiceTitanError.</returns>
            public static object createCampaignCost(Types.Marketing.CreateCostRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/costs";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Marketing.CampaignCostModel results = JsonConvert.DeserializeObject<Types.Marketing.CampaignCostModel>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an existing Campaign Cost.
            ///             ''' </summary>
            ///             ''' <param name="payload">The updated campaign cost.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, returns Nothing. Otherwise, returns a ServiceTitanError.</returns>
            public static object updateCampaignCost(Types.Marketing.UpdateCostRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/costs";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As CampaignCostModel = JsonConvert.DeserializeObject(Of CampaignCostModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return null;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new marketing campaign.
            ///             ''' </summary>
            ///             ''' <param name="payload">The campaign to create</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a DetailedCampaignModel (if successful), or a ServiceTitanError.</returns>
            public static object createCampaign(Types.Marketing.CampaignCreateModel payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/campaigns";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Marketing.DetailedCampaignModel results = JsonConvert.DeserializeObject<Types.Marketing.DetailedCampaignModel>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of marketing campaigns, based on your parameters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Returns either a paginated list of campaigns (CampaignModel_P) or a ServiceTitanError.</returns>
            public static object getCampaignList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/campaigns";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Marketing.CampaignModel_P results = JsonConvert.DeserializeObject<Types.Marketing.CampaignModel_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a campaign's costs.
            ///             ''' </summary>
            ///             ''' <param name="campaignId">The campaign ID to get the costs for.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Returns either a list of CampaignCostModel, or a ServiceTitanError.</returns>
            public static object getCampaignCosts(int campaignId, oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/campaigns/" + campaignId + "/costs";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    List<Types.Marketing.CampaignCostModel> results = JsonConvert.DeserializeObject<List<Types.Marketing.CampaignCostModel>>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single campaign, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="campaignId">The campaign's ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a single campaign (as DetailedCampaignModel) or a ServiceTitanError.</returns>
            public static object GetCampaign(int campaignId, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/campaigns/" + campaignId;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Marketing.DetailedCampaignModel results = JsonConvert.DeserializeObject<Types.Marketing.DetailedCampaignModel>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a campaign.
            ///             ''' </summary>
            ///             ''' <param name="campaignId">The campaign ID</param>
            ///             ''' <param name="payload">The campaign update class</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a DetailedCampaignModel, or a ServiceTitanError.</returns>
            public static object updateCampaign(int campaignId, Types.Marketing.CampaignCreateModel payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/marketing/v2/tenant/" + tenant + "/campaigns/" + campaignId;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Marketing.DetailedCampaignModel results = JsonConvert.DeserializeObject<Types.Marketing.DetailedCampaignModel>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class Memberships : Types.Memberships
        {
            /// <summary>
            ///             ''' Creates a membership sale invoice, thus also creating the membership. This endpoint is the equivalent of clicking the 'Sell Membership' button in the location page in the GUI.
            ///             ''' </summary>
            ///             ''' <param name="payload">The new membership sale</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a MembershipSaleInvoiceCreateResponse, or a ServiceTitanError.</returns>
            public static object sellMembership(Types.Memberships.MembershipSaleInvoiceCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/memberships/sale";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.MembershipSaleInvoiceCreateResponse results = JsonConvert.DeserializeObject<Types.Memberships.MembershipSaleInvoiceCreateResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of customer memberships, using options as a filter.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">List of options/filters</param>
            ///             ''' <returns>Either returns a paginated list of memberships (CustomerMembershipResponse_P) or a ServiceTitanError.</returns>
            public static object getMembershipList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/memberships";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.CustomerMembershipResponse_P results = JsonConvert.DeserializeObject<Types.Memberships.CustomerMembershipResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a membership record by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The membership ID.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns a single membership (CustomerMembershipResponse) or a ServiceTitanError.</returns>
            public static object getMembership(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/memberships/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.CustomerMembershipResponse results = JsonConvert.DeserializeObject<Types.Memberships.CustomerMembershipResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a membership with new information.
            ///             ''' </summary>
            ///             ''' <param name="id">The membership's ID.</param>
            ///             ''' <param name="payload">The new membership information</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a ModificationResponse (If successful), or a ServiceTitanError.</returns>
            public static object updateMembership(int id, Types.Memberships.CustomerMembershipUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/memberships/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.ModificationResponse results = JsonConvert.DeserializeObject<Types.Memberships.ModificationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new invoice template.
            ///             ''' </summary>
            ///             ''' <param name="payload">The new invoice template</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns a ModificationResponse (if successful) or a ServiceTitanError.</returns>
            public static object createInvoiceTemplate(Types.Memberships.InvoiceTemplateCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/invoice-templates";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.ModificationResponse results = JsonConvert.DeserializeObject<Types.Memberships.ModificationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets invoice templates using a list of IDs (Max: 50).
            ///             ''' </summary>
            ///             ''' <param name="ids">A list of invoice template IDs. (TODO: Find if they need to be comma-delimited, assuming yes)</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a paginated list of Invoice templates (InvoiceTemplateResponse_P) or a ServiceTitanError.</returns>
            public static object getInvoiceTemplatesByIds(string ids, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/invoice-templates?ids=" + System.Net.WebUtility.UrlEncode(ids);

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.InvoiceTemplateResponse_P results = JsonConvert.DeserializeObject<Types.Memberships.InvoiceTemplateResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single invoice template by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The Invoice Template ID.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns an InvoiceTemplateResponse, or a ServiceTitanError.</returns>
            public static object getInvoiceTemplate(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/invoice-templates/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.InvoiceTemplateResponse results = JsonConvert.DeserializeObject<Types.Memberships.InvoiceTemplateResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a single invoice template.
            ///             ''' </summary>
            ///             ''' <param name="id">The Invoice Template ID</param>
            ///             ''' <param name="payload">The Invoice Template Payload</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a ModificationResponse, or a ServiceTitanError.</returns>
            public static object updateInvoiceTemplate(int id, Types.Memberships.InvoiceTemplateUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/invoice-templates/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.ModificationResponse results = JsonConvert.DeserializeObject<Types.Memberships.ModificationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Returns a list of Recurring Service Events based on your parameters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters/parameters</param>
            ///             ''' <returns>Either returns a paginated list of Recurring Service Events (LocationRecurringServiceEventResponse_P) or a ServiceTitanError.</returns>
            public static object getRecurringServiceEvents(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/invoice-templates";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.LocationRecurringServiceEventResponse_P results = JsonConvert.DeserializeObject<Types.Memberships.LocationRecurringServiceEventResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Marking an event as complete links the job with provided JobID to the given Location Recurring Service Event. It will also copy over invoice items to the Job Invoice corresponding to the Invoice Template of the Location Recurring Service the Event was generated from.
            ///             ''' </summary>
            ///             ''' <param name="id">The recurring service event id.</param>
            ///             ''' <param name="payload">The payload (Containing the job id)</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, returns a ModificationResponse. Otherwise, returns a ServiceTitanError.</returns>
            public static object markRecurringServiceEventComplete(int id, Types.Memberships.MarkEventCompletedStatusUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/invoice-templates/" + id + "/mark-complete";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.ModificationResponse results = JsonConvert.DeserializeObject<Types.Memberships.ModificationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Marking an event as incomplete unlinks the job with provided JobID to the given Location Recurring Service Event. It will also delete the invoice items that were copied over when the Location Recurring Service Event was marked as completed on the Job.
            ///             ''' </summary>
            ///             ''' <param name="id">The recurring service event id.</param>
            ///             ''' <param name="payload">The payload (Containing the job id)</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, returns a ModificationResponse. Otherwise, returns a ServiceTitanError.</returns>
            public static object markRecurringServiceEventIncomplete(int id, Types.Memberships.MarkEventCompletedStatusUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/invoice-templates/" + id + "/mark-incomplete";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.ModificationResponse results = JsonConvert.DeserializeObject<Types.Memberships.ModificationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of recurring services.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">List of properties/filters</param>
            ///             ''' <returns>Either returns a paginated list of LocationRecurringServiceResponse (LocationRecurringServiceResponse_P) or a ServiceTitanError.</returns>
            public static object getRecurringServicesList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/recurring-services";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.LocationRecurringServiceResponse_P results = JsonConvert.DeserializeObject<Types.Memberships.LocationRecurringServiceResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Returns a single recurring service (By ID).
            ///             ''' </summary>
            ///             ''' <param name="id">The recurring service ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns a LocationRecurringServiceResponse, or a ServiceTitanError.</returns>
            public static object getRecurringService(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/recurring-services/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.LocationRecurringServiceResponse results = JsonConvert.DeserializeObject<Types.Memberships.LocationRecurringServiceResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates a single recurring service (By ID).
            ///             ''' </summary>
            ///             ''' <param name="id">The recurring service ID</param>
            ///             ''' <param name="payload">The update payload</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a ModificationResponse (If successful) or a ServiceTitanError.</returns>
            public static object updateRecurringService(int id, Types.Memberships.LocationRecurringServiceUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/recurring-services/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.ModificationResponse results = JsonConvert.DeserializeObject<Types.Memberships.ModificationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of membership types.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters for the API.</param>
            ///             ''' <returns>Returns either a list of membership types (MembershipTypeResponse_P) or a ServiceTitanError.</returns>
            public static object getMembershipTypesList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/membership-types";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.MembershipTypeResponse_P results = JsonConvert.DeserializeObject<Types.Memberships.MembershipTypeResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets discounts for the given membership type
            ///             ''' </summary>
            ///             ''' <param name="id">Membership type ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a list of Membership Type Discounts (List of(MembershipTypeDiscountItemResponse)) or a ServiceTitanError.</returns>
            public static object getMembershipTypeDiscounts(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/membership-types/" + id + "/discounts";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    List<Types.Memberships.MembershipTypeDiscountItemResponse> results = JsonConvert.DeserializeObject<List<Types.Memberships.MembershipTypeDiscountItemResponse>>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets duration/billing options for the given membership type
            ///             ''' </summary>
            ///             ''' <param name="id">The membership Type ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/parameters (For this endpoint, only 'active' is accepted)</param>
            ///             ''' <returns>Returns either a 'List(Of MembershipTypeDurationBillingItemResponse)' or a ServiceTitanError.</returns>
            public static object getMembershipTypeDurationBillingItems(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/membership-types/" + id + "/duration-billing-items";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    List<Types.Memberships.MembershipTypeDurationBillingItemResponse> results = JsonConvert.DeserializeObject<List<Types.Memberships.MembershipTypeDurationBillingItemResponse>>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single membership type (By it's ID)
            ///             ''' </summary>
            ///             ''' <param name="id">The membership type's ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a MembershipTypeResponse, or a ServiceTitanError.</returns>
            public static object getMembershipType(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/membership-types/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.MembershipTypeResponse results = JsonConvert.DeserializeObject<Types.Memberships.MembershipTypeResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of Service Items for a specific Membership Type's Recurring event.
            ///             ''' </summary>
            ///             ''' <param name="id">The membership type ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a 'List (Of MembershipTypeRecurringServiceItemResponse)' or a ServiceTitanError.</returns>
            public static object getMembershipTypeRecurringServiceItems(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/membership-types/" + id + "/recurring-service-items";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    List<Types.Memberships.MembershipTypeRecurringServiceItemResponse> results = JsonConvert.DeserializeObject<List<Types.Memberships.MembershipTypeRecurringServiceItemResponse>>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a paginated list of Recurring Service Types.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/parameters</param>
            ///             ''' <returns>Either returns a paginated list of recurring service types (RecurringServiceTypeResponse_P) or a ServiceTitanError.</returns>
            public static object getRecurringServiceTypeList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/recurring-service-types";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.RecurringServiceTypeResponse_P results = JsonConvert.DeserializeObject<Types.Memberships.RecurringServiceTypeResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single Recurring Service Type by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="id">The recurring service type's ID.</param>
            ///             ''' <returns>Either returns a RecurringServiceTypeResponse or a ServiceTitanError.</returns>

            public static object getRecurringServiceType(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/memberships/v2/tenant/" + tenant + "/recurring-service-types/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Memberships.RecurringServiceTypeResponse results = JsonConvert.DeserializeObject<Types.Memberships.RecurringServiceTypeResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class Payroll : Types.Payroll
        {
            /// <summary>
            ///             ''' Gets a list of activity codes.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of options/parameters.</param>
            ///             ''' <returns>Either returns a paginated list of activity codes (PayrollActivityCodeResponse_P) or a ServiceTitanError.</returns>
            public static object getActivityCodeList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/activity-codes";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.PayrollActivityCodeResponse_P results = JsonConvert.DeserializeObject<Types.Payroll.PayrollActivityCodeResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single activity code, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="id">The activity code ID</param>
            ///             ''' <returns>Either returns a single activity code (PayrollActivityCodeResponse) or a ServiceTitanError.</returns>

            public static object getActivityCode(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/activity-codes/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.PayrollActivityCodeResponse results = JsonConvert.DeserializeObject<Types.Payroll.PayrollActivityCodeResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new Gross Pay Item record.
            ///             ''' </summary>
            ///             ''' <param name="payload">The gross pay item create request</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns the ID of the created item (In the form of a ModificationResponse) or a ServiceTitanError.</returns>
            public static object createGrossPayItem(Types.Payroll.GrossPayItemCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/gross-pay-items";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If

                    Console.WriteLine("Payload: " + JsonConvert.SerializeObject(payload, new JsonSerializerSettings{NullValueHandling = NullValueHandling.Ignore, Formatting = Formatting.Indented}));
                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.ModificationResponse results = JsonConvert.DeserializeObject<Types.Payroll.ModificationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Deletes a specific Gross Pay Item.
            ///             ''' </summary>
            ///             ''' <param name="id">The Gross Pay Item's ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns nothing (If successful) or returns a ServiceTitanError.</returns>
            public static object deleteGrossPayItem(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/gross-pay-items/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    object results = null;
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of gross pay items matching what is requested.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of filters/parameters</param>
            ///             ''' <returns>Either returns a paginated list of gross pay items (GrossPayItemResponse_P) or a ServiceTitanError.</returns>
            public static object getGrossPayItemList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/gross-pay-items";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.GrossPayItemResponse_P results = JsonConvert.DeserializeObject<Types.Payroll.GrossPayItemResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an existing Gross Pay Item record
            ///             ''' </summary>
            ///             ''' <param name="payload">The updated Gross Pay Item</param>
            ///             ''' <param name="id">The Gross Pay Item ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>If successful, returns a ModificationResponse. If not, returns a ServiceTitanError.</returns>
            public static object updateGrossPayItem(Types.Payroll.GrossPayItemCreateRequest payload, int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/gross-pay-items/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.ModificationResponse results = JsonConvert.DeserializeObject<Types.Payroll.ModificationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });

                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of splits for a specific job id.
            ///             ''' </summary>
            ///             ''' <param name="id">The job ID.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a list of JobSplitResponse, or a ServiceTitanError.</returns>
            public static object getJobSplits(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/jobs/" + id + "/splits";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    List<Types.Payroll.JobSplitResponse> results = JsonConvert.DeserializeObject<List<Types.Payroll.JobSplitResponse>>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new payroll adjustment.
            ///             ''' </summary>
            ///             ''' <param name="payload">The payroll adjustment</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a ModificationResponse or a ServiceTitanError.</returns>
            public static object createPayrollAdjustment(Types.Payroll.PayrollAdjustmentCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/payroll-adjustments";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.ModificationResponse results = JsonConvert.DeserializeObject<Types.Payroll.ModificationResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of payroll adjustments, based on your parameters/filters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Either returns a list of payroll adjustments (PayrollAdjustmentResponse_P) or a ServiceTitanError.</returns>
            public static object getPayrollAdjustmentList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/payroll-adjustments";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.PayrollAdjustmentResponse_P results = JsonConvert.DeserializeObject<Types.Payroll.PayrollAdjustmentResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a specific payroll adjustment record, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The payroll adjustment ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a PayrollAdjustmentResponse, or a ServiceTitanError.</returns>
            public static object getPayrollAdjustment(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/payroll-adjustments/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.PayrollAdjustmentResponse results = JsonConvert.DeserializeObject<Types.Payroll.PayrollAdjustmentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets an employee's payroll records, based on the employee's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The employee's ID.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Either returns a paginated list of Payroll records (PayrollResponse_P) or a ServiceTitanError.</returns>
            public static object getEmployeePayrollRecords(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/employees/" + id + "/payrolls";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.PayrollResponse_P results = JsonConvert.DeserializeObject<Types.Payroll.PayrollResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets payroll records.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Either returns a paginated list of Payroll records (PayrollResponse_P) or a ServiceTitanError.</returns>
            public static object getPayrollRecords(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/payrolls";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.PayrollResponse_P results = JsonConvert.DeserializeObject<Types.Payroll.PayrollResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a technician's payroll records, based on the employee's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The technician's ID.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Either returns a paginated list of Payroll records (PayrollResponse_P) or a ServiceTitanError.</returns>
            public static object getTechnicianPayrollRecords(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/technicians/" + id + "/payrolls";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.PayrollResponse_P results = JsonConvert.DeserializeObject<Types.Payroll.PayrollResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of timesheet codes
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters.</param>
            ///             ''' <returns>Either returns a paginated list of timesheet codes (TimesheetCodeResponse_P) or a ServiceTitanError.</returns>
            public static object getTimesheetCodes(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/timesheet-codes";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.TimesheetCodeResponse_P results = JsonConvert.DeserializeObject<Types.Payroll.TimesheetCodeResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a specific timesheet code, by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The timesheet code ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a TimesheetCodeResponse or a ServiceTitanError.</returns>
            public static object getTimesheetCode(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/payroll/v2/tenant/" + tenant + "/timesheet-codes/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Payroll.TimesheetCodeResponse results = JsonConvert.DeserializeObject<Types.Payroll.TimesheetCodeResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class Pricebook : Types.Pricebook
        {
            /// <summary>
            ///             ''' Creates a new category in the pricebook.
            ///             ''' </summary>
            ///             ''' <param name="payload">The new category to add</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either the created category (CategoryResponse) or a ServiceTitanError.</returns>
            public static object createCategory(Types.Pricebook.CategoryCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/categories";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.CategoryResponse results = JsonConvert.DeserializeObject<Types.Pricebook.CategoryResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Deletes a category from the pricebook using it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The category ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>IF successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            public static object deleteCategory(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/categories/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As CategoryResponse = JsonConvert.DeserializeObject(Of CategoryResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    object results = null;
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a category, using it's ID
            ///             ''' </summary>
            ///             ''' <param name="id">The category ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a CategoryResponse, or a ServiceTitanError.</returns>
            public static object getCategory(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/categories/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.CategoryResponse results = JsonConvert.DeserializeObject<Types.Pricebook.CategoryResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of categories, depending on your parameters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/options</param>
            ///             ''' <returns>Either returns a paginated list of categories (CategoryResponse_P) or a ServiceTitanError.</returns>
            public static object getCategoryList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/categories";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.CategoryResponse_P results = JsonConvert.DeserializeObject<Types.Pricebook.CategoryResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an existing category
            ///             ''' </summary>
            ///             ''' <param name="id">The category's ID</param>
            ///             ''' <param name="payload">The updated category</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a CategoryResponse, or a ServiceTitanError.</returns>
            public static object updateCategory(int id, Types.Pricebook.CategoryUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/categories/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.CategoryResponse results = JsonConvert.DeserializeObject<Types.Pricebook.CategoryResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }

            /// <summary>
            ///             ''' Creates a new discount and/or fee in the pricebook.
            ///             ''' </summary>
            ///             ''' <param name="payload">The new discount and/or fee to add</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either the created discount and/or fee (DiscountAndFeesResponse) or a ServiceTitanError.</returns>
            public static object createDiscountAndFees(Types.Pricebook.DiscountAndFeesCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/discounts-and-fees";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.DiscountAndFeesResponse results = JsonConvert.DeserializeObject<Types.Pricebook.DiscountAndFeesResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Deletes a discount and/or fee from the pricebook using it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The discount and/or fee ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>IF successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            public static object deleteDiscountAndFees(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/discounts-and-fees/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As DiscountAndFeesResponse = JsonConvert.DeserializeObject(Of DiscountAndFeesResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    object results = null;
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a discount and/or fee, using it's ID
            ///             ''' </summary>
            ///             ''' <param name="id">The discount and/or fee ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a DiscountAndFeesResponse, or a ServiceTitanError.</returns>
            public static object getDiscountAndFees(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/discounts-and-fees/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.DiscountAndFeesResponse results = JsonConvert.DeserializeObject<Types.Pricebook.DiscountAndFeesResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of categories, depending on your parameters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/options</param>
            ///             ''' <returns>Either returns a paginated list of categories (DiscountAndFeesResponse_P) or a ServiceTitanError.</returns>
            public static object getDiscountAndFeesList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/discounts-and-fees";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.DiscountAndFeesResponse_P results = JsonConvert.DeserializeObject<Types.Pricebook.DiscountAndFeesResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an existing discount and/or fee
            ///             ''' </summary>
            ///             ''' <param name="id">The discount and/or fee's ID</param>
            ///             ''' <param name="payload">The updated discount and/or fee</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a DiscountAndFeesResponse, or a ServiceTitanError.</returns>
            public static object updateDiscountAndFees(int id, Types.Pricebook.DiscountAndFeesUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/discounts-and-fees/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.DiscountAndFeesResponse results = JsonConvert.DeserializeObject<Types.Pricebook.DiscountAndFeesResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }

            /// <summary>
            ///             ''' Creates a new equipment in the pricebook.
            ///             ''' </summary>
            ///             ''' <param name="payload">The new equipment to add</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either the created equipment (EquipmentResponse) or a ServiceTitanError.</returns>
            public static object createEquipment(Types.Pricebook.EquipmentCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/equipment";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.EquipmentResponse results = JsonConvert.DeserializeObject<Types.Pricebook.EquipmentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Deletes a equipment from the pricebook using it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The equipment ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>IF successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            public static object deleteEquipment(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/equipment/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As EquipmentResponse = JsonConvert.DeserializeObject(Of EquipmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    object results = null;
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a equipment, using it's ID
            ///             ''' </summary>
            ///             ''' <param name="id">The equipment ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a EquipmentResponse, or a ServiceTitanError.</returns>
            public static object getEquipment(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/equipment/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.EquipmentResponse results = JsonConvert.DeserializeObject<Types.Pricebook.EquipmentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of categories, depending on your parameters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/options</param>
            ///             ''' <returns>Either returns a paginated list of categories (EquipmentResponse_P) or a ServiceTitanError.</returns>
            public static object getEquipmentList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/equipment";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.EquipmentResponse_P results = JsonConvert.DeserializeObject<Types.Pricebook.EquipmentResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an existing equipment
            ///             ''' </summary>
            ///             ''' <param name="id">The equipment's ID</param>
            ///             ''' <param name="payload">The updated equipment</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a EquipmentResponse, or a ServiceTitanError.</returns>
            public static object updateEquipment(int id, Types.Pricebook.EquipmentUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/equipment/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.EquipmentResponse results = JsonConvert.DeserializeObject<Types.Pricebook.EquipmentResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }

            /// <summary>
            ///             ''' Creates a new material in the pricebook.
            ///             ''' </summary>
            ///             ''' <param name="payload">The new material to add</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either the created material (MaterialResponse) or a ServiceTitanError.</returns>
            public static object createMaterial(Types.Pricebook.MaterialCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/materials";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.MaterialResponse results = JsonConvert.DeserializeObject<Types.Pricebook.MaterialResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Deletes a material from the pricebook using it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The material ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>IF successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            public static object deleteMaterial(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/materials/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As MaterialResponse = JsonConvert.DeserializeObject(Of MaterialResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    object results = null;
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a material, using it's ID
            ///             ''' </summary>
            ///             ''' <param name="id">The material ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a MaterialResponse, or a ServiceTitanError.</returns>
            public static object getMaterial(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/materials/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.MaterialResponse results = JsonConvert.DeserializeObject<Types.Pricebook.MaterialResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of categories, depending on your parameters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/options</param>
            ///             ''' <returns>Either returns a paginated list of categories (MaterialResponse_P) or a ServiceTitanError.</returns>
            public static object getMaterialList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/materials";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.MaterialResponse_P results = JsonConvert.DeserializeObject<Types.Pricebook.MaterialResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an existing material
            ///             ''' </summary>
            ///             ''' <param name="id">The material's ID</param>
            ///             ''' <param name="payload">The updated material</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a MaterialResponse, or a ServiceTitanError.</returns>
            public static object updateMaterial(int id, Types.Pricebook.MaterialUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/materials/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.MaterialResponse results = JsonConvert.DeserializeObject<Types.Pricebook.MaterialResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }

            /// <summary>
            ///             ''' Creates a new service in the pricebook.
            ///             ''' </summary>
            ///             ''' <param name="payload">The new service to add</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either the created service (ServiceResponse) or a ServiceTitanError.</returns>
            public static object createService(Types.Pricebook.ServiceCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/services";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.ServiceResponse results = JsonConvert.DeserializeObject<Types.Pricebook.ServiceResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Deletes a service from the pricebook using it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The service ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>IF successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            public static object deleteService(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/services/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As ServiceResponse = JsonConvert.DeserializeObject(Of ServiceResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    object results = null;
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a service, using it's ID
            ///             ''' </summary>
            ///             ''' <param name="id">The service ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a ServiceResponse, or a ServiceTitanError.</returns>
            public static object getService(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/services/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.ServiceResponse results = JsonConvert.DeserializeObject<Types.Pricebook.ServiceResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of categories, depending on your parameters.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/options</param>
            ///             ''' <returns>Either returns a paginated list of categories (ServiceResponse_P) or a ServiceTitanError.</returns>
            public static object getServiceList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/services";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.ServiceResponse_P results = JsonConvert.DeserializeObject<Types.Pricebook.ServiceResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an existing service
            ///             ''' </summary>
            ///             ''' <param name="id">The service's ID</param>
            ///             ''' <param name="payload">The updated service</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a ServiceResponse, or a ServiceTitanError.</returns>
            public static object updateService(int id, Types.Pricebook.ServiceUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/pricebook/v2/tenant/" + tenant + "/services/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PATCH";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Pricebook.ServiceResponse results = JsonConvert.DeserializeObject<Types.Pricebook.ServiceResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class SalesAndEstimates : Types.SalesAndEstimates
        {
            /// <summary>
            ///             ''' Creates a new estimate.
            ///             ''' </summary>
            ///             ''' <param name="payload">The new estimate to create</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns an EstimateResponse, or a ServiceTitanError.</returns>
            public static object createEstimate(Types.SalesAndEstimates.CreateEstimateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/sales/v2/tenant/" + tenant + "/estimates";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.SalesAndEstimates.EstimateResponse results = JsonConvert.DeserializeObject<Types.SalesAndEstimates.EstimateResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Delete an estimate's item.
            ///             ''' </summary>
            ///             ''' <param name="estimateId">The estimate ID</param>
            ///             ''' <param name="estimateItemId">The estimate item ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns Nothing if successful. Otherwise, returns a ServiceTitanError.</returns>
            public static object deleteEstimateItem(int estimateId, int estimateItemId, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/sales/v2/tenant/" + tenant + "/estimates/" + estimateId + "/items/" + estimateItemId;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "DEL";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As EstimateResponse = JsonConvert.DeserializeObject(Of EstimateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    object results = null;
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Dismisses an active estimate.
            ///             ''' </summary>
            ///             ''' <param name="estimateId">The estimate's ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns Nothing if successful. Otherwise, returns a ServiceTitanError.</returns>
            public static object dismissEstimate(int estimateId, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/sales/v2/tenant/" + tenant + "/estimates/" + estimateId;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As EstimateResponse = JsonConvert.DeserializeObject(Of EstimateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    object results = null;
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             Gets an estimate by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="estimateId">The estimate ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns an EstimateResponse, or a ServiceTitanError.</returns>
            public static object getEstimate(int estimateId, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/sales/v2/tenant/" + tenant + "/estimates/" + estimateId;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.SalesAndEstimates.EstimateResponse results = JsonConvert.DeserializeObject<Types.SalesAndEstimates.EstimateResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of estimate items for an estimate.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Either returns a paginated list of estimate items (EstimateItemResponse_P) or a ServiceTitanError.</returns>
            public static object getEstimateItemList(int estimateid, oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/sales/v2/tenant/" + tenant + "/estimates/" + estimateid + "/items";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.SalesAndEstimates.EstimateItemResponse_P results = JsonConvert.DeserializeObject<Types.SalesAndEstimates.EstimateItemResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of estimates.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of settings/parameters</param>
            ///             ''' <returns>Returns either a paginated list of estimates (EstimateResponse_P) or a ServiceTitanError.</returns>
            public static object getEstimateList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/sales/v2/tenant/" + tenant + "/estimates/";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.SalesAndEstimates.EstimateResponse_P results = JsonConvert.DeserializeObject<Types.SalesAndEstimates.EstimateResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an estimate item on an estimate.
            ///             ''' </summary>
            ///             ''' <param name="estimateid">The estimate ID</param>
            ///             ''' <param name="payload">The updated estimate item</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either an EstimateItemUpdateResponse, or a ServiceTitanError.</returns>
            public static object updateEstimateItem(int estimateid, Types.SalesAndEstimates.EstimateItemCreateUpdateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/sales/v2/tenant/" + tenant + "/estimates/" + estimateid;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.SalesAndEstimates.EstimateItemUpdateResponse results = JsonConvert.DeserializeObject<Types.SalesAndEstimates.EstimateItemUpdateResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Sets an estimate as being 'sold'.
            ///             ''' </summary>
            ///             ''' <param name="estimateid">The estimate ID</param>
            ///             ''' <param name="payload">The SellRequest (Containing the soldBy ID)</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns an EstimateResponse, or a ServiceTitanError.</returns>
            public static object sellEstimate(int estimateid, Types.SalesAndEstimates.SellRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/sales/v2/tenant/" + tenant + "/estimates/" + estimateid + "/sell";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.SalesAndEstimates.EstimateResponse results = JsonConvert.DeserializeObject<Types.SalesAndEstimates.EstimateResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Unsells an estimate.
            ///             ''' </summary>
            ///             ''' <param name="estimateid">The estimate ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns Nothing (If successful), or a ServiceTitanError.</returns>
            public static object unsellEstimate(int estimateid, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/sales/v2/tenant/" + tenant + "/estimates/" + estimateid + "/unsell";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    // Dim results As EstimateResponse = JsonConvert.DeserializeObject(Of EstimateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    object results = null;
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Updates an estimate
            ///             ''' </summary>
            ///             ''' <param name="estimateid">The estimate ID</param>
            ///             ''' <param name="payload">The updated estimate payload</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either an EstimateResponse, or a ServiceTitanError.</returns>
            public static object updateEstimate(int estimateid, Types.SalesAndEstimates.UpdateEstimateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/sales/v2/tenant/" + tenant + "/estimates/" + estimateid;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.SalesAndEstimates.EstimateResponse results = JsonConvert.DeserializeObject<Types.SalesAndEstimates.EstimateResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class Settings : Types.Settings
        {
            /// <summary>
            ///             ''' Gets a single business unit by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The business unit ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a BusinessUnitResponse, or a ServiceTitanError.</returns>
            public static object getBusinessUnit(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/settings/v2/tenant/" + tenant + "/business-units/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Settings.BusinessUnitResponse results = JsonConvert.DeserializeObject<Types.Settings.BusinessUnitResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of business units based on your parameters
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Either returns a paginated list of business units (BusinessUnitResponse_P) or a ServiceTitanError.</returns>
            public static object getBusinessUnitList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/settings/v2/tenant/" + tenant + "/business-units";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Settings.BusinessUnitResponse_P results = JsonConvert.DeserializeObject<Types.Settings.BusinessUnitResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single employee by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The employee ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a EmployeeResponse, or a ServiceTitanError.</returns>
            public static object getEmployee(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/settings/v2/tenant/" + tenant + "/employees/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Settings.EmployeeResponse results = JsonConvert.DeserializeObject<Types.Settings.EmployeeResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of employees based on your parameters
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Either returns a paginated list of employees (EmployeeResponse_P) or a ServiceTitanError.</returns>
            public static object getEmployeeList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/settings/v2/tenant/" + tenant + "/employees";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Settings.EmployeeResponse_P results = JsonConvert.DeserializeObject<Types.Settings.EmployeeResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }

            /// <summary>
            ///             ''' Gets a list of tag types based on your parameters
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Either returns a paginated list of tag types (TagTypeResponse_P) or a ServiceTitanError.</returns>
            public static object getTagTypesList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/settings/v2/tenant/" + tenant + "/employees";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Settings.TagTypeResponse_P results = JsonConvert.DeserializeObject<Types.Settings.TagTypeResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single technician by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The technician ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a TechnicianResponse, or a ServiceTitanError.</returns>
            public static object getTechnician(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/settings/v2/tenant/" + tenant + "/technicians/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Settings.TechnicianResponse results = JsonConvert.DeserializeObject<Types.Settings.TechnicianResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of technicians based on your parameters
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Either returns a paginated list of technicians (TechnicianResponse_P) or a ServiceTitanError.</returns>
            public static object getTechnicianList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/settings/v2/tenant/" + tenant + "/technicians";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Settings.TechnicianResponse_P results = JsonConvert.DeserializeObject<Types.Settings.TechnicianResponse_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class TaskManagement : Types.TaskManagement
        {
            /// <summary>
            ///             ''' Gets all of the task management types in one fell swoop.
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns all of the data (ClientSideDataResponse) or a ServiceTitanError.</returns>
            public static object getTaskManagementData(oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/taskmanagement/v2/tenant/" + tenant + "/data";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.TaskManagement.ClientSideDataResponse results = JsonConvert.DeserializeObject<Types.TaskManagement.ClientSideDataResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new Task Management task
            ///             ''' </summary>
            ///             ''' <param name="payload">The new task to create</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a TaskCreateResponse, or a ServiceTitanError.</returns>
            public static object createTask(Types.TaskManagement.TaskCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/taskmanagement/v2/tenant/" + tenant + "/tasks";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.TaskManagement.TaskCreateResponse results = JsonConvert.DeserializeObject<Types.TaskManagement.TaskCreateResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a Task Management subtask
            ///             ''' </summary>
            ///             ''' <param name="id">The main task's ID</param>
            ///             ''' <param name="payload">The subtask to create</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a SubTaskCreateResponse, or a ServiceTitanError.</returns>
            public static object createSubtask(int id, Types.TaskManagement.SubtaskCreateRequest payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/taskmanagement/v2/tenant/" + tenant + "/tasks/" + id + "/subtasks";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.TaskManagement.SubTaskCreateResponse results = JsonConvert.DeserializeObject<Types.TaskManagement.SubTaskCreateResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class Telecom : Types.Telecom
        
        {
            public static TimeSpan convertCallDurationToTimeSpan(string duration) {
                var parts = duration.Split(":");
                Console.WriteLine("Converting " + duration + " to timespan.");
                TimeSpan value = new TimeSpan(System.Convert.ToInt32(parts[0]), System.Convert.ToInt32(parts[1]), System.Convert.ToInt32(Math.Round(System.Convert.ToDecimal(parts[2]))));
                return value;
            }
            /// <summary>
            ///             ''' Gets a list of call reasons
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters</param>
            ///             ''' <returns>Either returns a paginated list of call reasons (CallReasonResponse_P) or a ServiceTitanError.</returns>
            public static object getCallReasonList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/telecom/v2/tenant/" + tenant + "/call-reasons";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Telecom.CallReasonResponse results = JsonConvert.DeserializeObject<Types.Telecom.CallReasonResponse>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Creates a new call record. (NOTE: Does not actually initiate a call)
            ///             ''' </summary>
            ///             ''' <param name="payload">The new call record</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a DetailedCallModel, or a ServiceTitanError.</returns>
            public static object createCall(Types.Telecom.CallInModel payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/telecom/v2/tenant/" + tenant + "/call-reasons";

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "POST";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Telecom.DetailedCallModel results = JsonConvert.DeserializeObject<Types.Telecom.DetailedCallModel>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a list of calls
            ///             ''' </summary>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <param name="options">A list of parameters/filters.</param>
            ///             ''' <returns>Either returns a paginated list of calls (BundleCallModel_P) or a ServiceTitanError.</returns>
            public static object getCallList(oAuth2.AccessToken accesstoken, string STAppKey, int tenant, List<Internal.OptionsList> options = null)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/telecom/v2/tenant/" + tenant + "/calls";

                    int counter = 0;
                    if (options != null)
                    {
                        if (options.Count > 0)
                        {
                            counter = 1;
                            foreach (var item in options)
                            {
                                if (counter == 1)
                                    baseurl += "?" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                else
                                    baseurl += "&" + System.Net.WebUtility.UrlEncode(item.key) + "=" + System.Net.WebUtility.UrlEncode(item.value);
                                counter += 1;
                            }
                        }
                    }


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Telecom.BundleCallModel_P results = JsonConvert.DeserializeObject<Types.Telecom.BundleCallModel_P>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a single call by it's ID.
            ///             ''' </summary>
            ///             ''' <param name="id">The Call ID</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Either returns a DetailedBundleCallModel, or a ServiceTitanError.</returns>
            public static object getCall(int id, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/telecom/v2/tenant/" + tenant + "/calls/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "GET";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    // Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                    // req.ContentLength = bytearray.Length
                    // req.Timeout = 999999
                    // Dim datastream As Stream = req.GetRequestStream
                    // datastream.Write(bytearray, 0, bytearray.Length)
                    // datastream.Close()

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Telecom.DetailedBundleCallModel results = JsonConvert.DeserializeObject<Types.Telecom.DetailedBundleCallModel>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
            /// <summary>
            ///             ''' Gets a call recording, and saves it on disk. (mp3)
            ///             ''' </summary>
            ///             ''' <param name="id">The call ID</param>
            ///             ''' <param name="saveLocation">The full path on disk where to save the file.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>IF the file is not found, a webclient exception will be thrown. If successful, the file will be saved on disk.</returns>
            public static void getCallRecording(int id, string saveLocation, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                TimeSpan timespan = DateTime.Now - lastQuery;
                if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                    // Try to avoid getting hit by the rate limiter by sleeping it off
                    System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                string domain;
                if (useSandbox == true)
                    domain = "https://" + sandboxEnvironment;
                else
                    domain = "https://" + productionEnvironment;
                string baseurl = domain + "/telecom/v2/tenant/" + tenant + "/calls/" + id + "/recording";

                // Dim counter As Integer = 0
                // If options IsNot Nothing Then
                // If options.Count > 0 Then
                // counter = 1
                // For Each item In options
                // If counter = 1 Then
                // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                // Else
                // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                // End If
                // counter &= 1
                // Next
                // End If
                // End If


                Console.WriteLine("Executing: " + baseurl);
                WebClient client = new WebClient();
                client.Headers.Add("ST-App-Key", STAppKey);
                client.Headers.Add("Authorization", accesstoken.access_token);
                client.DownloadFile(baseurl, saveLocation);
            }
            /// <summary>
            ///             ''' Gets a call voicemail, and saves it on disk. (mp3)
            ///             ''' </summary>
            ///             ''' <param name="id">The call ID</param>
            ///             ''' <param name="saveLocation">The full path on disk where to save the file.</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>IF the file is not found, a webclient exception will be thrown. If successful, the file will be saved on disk.</returns>
            public static void getCallVoicemail(int id, string saveLocation, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                TimeSpan timespan = DateTime.Now - lastQuery;
                if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                    // Try to avoid getting hit by the rate limiter by sleeping it off
                    System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                string domain;
                if (useSandbox == true)
                    domain = "https://" + sandboxEnvironment;
                else
                    domain = "https://" + productionEnvironment;
                string baseurl = domain + "/telecom/v2/tenant/" + tenant + "/calls/" + id + "/voicemail";

                // Dim counter As Integer = 0
                // If options IsNot Nothing Then
                // If options.Count > 0 Then
                // counter = 1
                // For Each item In options
                // If counter = 1 Then
                // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                // Else
                // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                // End If
                // counter &= 1
                // Next
                // End If
                // End If


                Console.WriteLine("Executing: " + baseurl);
                WebClient client = new WebClient();
                client.Headers.Add("ST-App-Key", STAppKey);
                client.Headers.Add("Authorization", accesstoken.access_token);
                client.DownloadFile(baseurl, saveLocation);
            }
            /// <summary>
            ///             ''' Updates a call record.
            ///             ''' </summary>
            ///             ''' <param name="id">The Call ID</param>
            ///             ''' <param name="payload">The updated call payload</param>
            ///             ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ///             ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ///             ''' <param name="tenant">Your Tenant ID</param>
            ///             ''' <returns>Returns either a DetailedCallModel, or a ServiceTitanError.</returns>
            public static object updateCall(int id, Types.Telecom.CallInUpdateModelV2 payload, oAuth2.AccessToken accesstoken, string STAppKey, int tenant)
            {
                try
                {
                    TimeSpan timespan = DateTime.Now - lastQuery;
                    if (lastQuery != DateTime.MinValue & timespan.TotalMilliseconds < minMsSinceLastQuery)
                        // Try to avoid getting hit by the rate limiter by sleeping it off
                        System.Threading.Thread.Sleep((minMsSinceLastQuery - (int)timespan.TotalMilliseconds) + 100);

                    string domain;
                    if (useSandbox == true)
                        domain = "https://" + sandboxEnvironment;
                    else
                        domain = "https://" + productionEnvironment;
                    string baseurl = domain + "/telecom/v2/tenant/" + tenant + "/calls/" + id;

                    // Dim counter As Integer = 0
                    // If options IsNot Nothing Then
                    // If options.Count > 0 Then
                    // counter = 1
                    // For Each item In options
                    // If counter = 1 Then
                    // baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // Else
                    // baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                    // End If
                    // counter &= 1
                    // Next
                    // End If
                    // End If


                    Console.WriteLine("Executing: " + baseurl);
                    WebRequest req = WebRequest.Create(baseurl);
                    req.Method = "PUT";
                    req.Timeout = 999999;
                    req.Headers.Add("ST-App-Key", STAppKey);
                    req.Headers.Add("Authorization", accesstoken.access_token);
                    req.ContentType = "application/json";

                    byte[] bytearray = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload));
                    req.ContentLength = bytearray.Length;
                    req.Timeout = 999999;
                    Stream datastream = req.GetRequestStream();
                    datastream.Write(bytearray, 0, bytearray.Length);
                    datastream.Close();

                    WebResponse response = req.GetResponse();
                    Stream buffer = response.GetResponseStream();
                    StreamReader streamread = new StreamReader(buffer, System.Text.Encoding.UTF8);
                    string output = streamread.ReadToEnd();
                    Types.Telecom.DetailedCallModel results = JsonConvert.DeserializeObject<Types.Telecom.DetailedCallModel>(output, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
                    // Dim results As Object = Nothing
                    streamread.Close();
                    buffer.Close();
                    response.Close();
                    return results;
                }
                catch (WebException ex)
                {
                    ServiceTitanError newerror = ErrorHandling.ProcessError(ex);
                    return newerror;
                }
            }
        }
        public class Utility {
                public static Types.Pricebook.MaterialUpdateRequest ConvertMaterialRequestToUpdateRequest(Types.Pricebook.MaterialResponse material) {
                    var UpdatedMaterial = new Types.Pricebook.MaterialUpdateRequest();
                    UpdatedMaterial.account = material.account;
                    UpdatedMaterial.active = material.active;
                    UpdatedMaterial.addOnMemberPrice = material.addOnMemberPrice;
                    UpdatedMaterial.addOnPrice = material.addOnPrice;
                    
                    UpdatedMaterial.assetAccount = material.assetAccount;
                    UpdatedMaterial.assets = material.assets;
                    UpdatedMaterial.categories = material.categories;
                    UpdatedMaterial.code = material.code;
                    UpdatedMaterial.commissionBonus = material.commissionBonus;
                    UpdatedMaterial.cost = material.cost;
                    UpdatedMaterial.costOfSaleAccount = material.costOfSaleAccount;
                    UpdatedMaterial.deductAsJobCost = material.deductAsJobCost;
                    UpdatedMaterial.description = material.description;
                    UpdatedMaterial.displayName = material.displayName;
                    UpdatedMaterial.hours = material.hours;
                    UpdatedMaterial.isInventory = material.isInventory;
                    UpdatedMaterial.memberPrice = material.memberPrice;
                    UpdatedMaterial.otherVendors = new List<Types.Pricebook.SkuVendorRequest>();
                    if (material.otherVendors != null) {
                        foreach(var items in material.otherVendors) {
                            Types.Pricebook.SkuVendorSubAccountRequest subaccount = null;

                            if (items.primarySubAccount != null) {
                                subaccount = new Types.Pricebook.SkuVendorSubAccountRequest();
                                subaccount.accountName = items.primarySubAccount.accountName;
                                subaccount.cost = items.primarySubAccount.cost;
                            }
                            List<Types.Pricebook.SkuVendorSubAccountRequest> otherSubAccounts = null;
                            if (items.otherSubAccounts != null) {
                                otherSubAccounts = new List<Types.Pricebook.SkuVendorSubAccountRequest>();
                                foreach (var accounts in items.otherSubAccounts) {
                                    otherSubAccounts.Add(new Types.Pricebook.SkuVendorSubAccountRequest {accountName = accounts.accountName, cost = accounts.cost});
                                }
                            }
                            
                            UpdatedMaterial.otherVendors.Add(new Types.Pricebook.SkuVendorRequest{vendorId = items.vendorId, memo = items.memo, vendorPart = items.vendorPart, cost = items.cost, active = items.active, primarySubAccount = subaccount,  otherSubAccounts = otherSubAccounts});
                        }
                    }
                    UpdatedMaterial.paysCommission = material.paysCommission;
                    UpdatedMaterial.price = material.price;
                    if (material.primaryVendor != null) {
                        UpdatedMaterial.primaryVendor = new Types.Pricebook.SkuVendorRequest();
                        UpdatedMaterial.primaryVendor.active = material.primaryVendor.active;
                        UpdatedMaterial.primaryVendor.cost = material.primaryVendor.cost;
                        UpdatedMaterial.primaryVendor.memo = material.primaryVendor.memo;
                        Types.Pricebook.SkuVendorSubAccountRequest subaccount = null;
                        if (material.primaryVendor.primarySubAccount != null) {
                            subaccount = new Types.Pricebook.SkuVendorSubAccountRequest();
                            subaccount.accountName = material.primaryVendor.primarySubAccount.accountName;
                            subaccount.cost = material.primaryVendor.primarySubAccount.cost;
                        }
                        List<Types.Pricebook.SkuVendorSubAccountRequest> otherSubAccounts = null;
                        if (material.primaryVendor.otherSubAccounts != null) {
                            otherSubAccounts= new List<Types.Pricebook.SkuVendorSubAccountRequest>();
                            foreach (var accounts in material.primaryVendor.otherSubAccounts) {
                                otherSubAccounts.Add(new Types.Pricebook.SkuVendorSubAccountRequest {accountName = accounts.accountName, cost = accounts.cost});
                            }
                        }
                        UpdatedMaterial.primaryVendor.otherSubAccounts = otherSubAccounts;
                        UpdatedMaterial.primaryVendor.primarySubAccount = subaccount;
                        UpdatedMaterial.primaryVendor.vendorId = material.primaryVendor.vendorId;
                        UpdatedMaterial.primaryVendor.vendorPart = material.primaryVendor.vendorPart;
                    }
                    UpdatedMaterial.taxable = material.taxable;
                    UpdatedMaterial.unitOfMeasure = material.unitOfMeasure;
                    return UpdatedMaterial;
                }
            }
    }
    public class Internal
    {
        /// <summary>
        ///         OptionsList: Used to enumerate HTTP arguments, mainly used when GETting data from the API as a way to filter the data returned.
        ///         </summary>
        public class OptionsList
        {
            public string key;
            public string value;
        }
    }
    public class ApiErrorResponse
    {
        public string type;
        public string title;
        public int status;
        public string traceId;
        public List<APIError> errors;
        public List<APIErrorData> data;
    }
    public class APIErrorData
    {
        public string ErrorCode;
    }
    public class APIError
    {
        [JsonProperty("")]
        public string desc;
    }
    public class ServiceTitanError
    {
        public ApiErrorResponse ApiErrorResponse = new ApiErrorResponse();
        public WebException WebError = new WebException();
    }
    public class ErrorHandling
    {
        public static ServiceTitanError ProcessError(WebException exception)
        {
            Console.WriteLine("STAPIv2: -----An Error Has Occured-----");
            var resp = exception.Response;
            

            
            Console.WriteLine("HTTP Request Code: " + (int)exception.Status);
            Stream str = exception.Response.GetResponseStream();
            StreamReader output = new StreamReader(str);
            string outputstr = output.ReadToEnd();
            Console.WriteLine("ServiceTitan returned: " + outputstr);
            Console.WriteLine("StackTrace: " + exception.StackTrace);
            Console.WriteLine("STAPIv2: -----End of Error-----");
            ServiceTitanError newerror = new ServiceTitanError();
            newerror.ApiErrorResponse = JsonConvert.DeserializeObject<ApiErrorResponse>(outputstr, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore });
            newerror.WebError = exception;
            return newerror;
        }
    }
}  
}

