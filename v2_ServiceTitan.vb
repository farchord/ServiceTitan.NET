Imports System.IO
Imports System.Net
Imports System.Text
Imports Newtonsoft.Json
Public Class ServiceTitan
    Public Shared lastQuery As DateTime
    Const minMsSinceLastQuery As Integer = 500
    Public Shared useSandbox As Boolean = True
    Const productionEnvironment As String = "api.servicetitan.io"
    Const productionAuthEnvironment As String = "auth.servicetitan.io"
    Const sandboxEnvironment As String = "api-integration.servicetitan.io"
    Const sandboxAuthEnvironment As String = "auth-integration.servicetitan.io"
    Public Class oAuth2
        Public Class Credentials
            ''' <summary>
            ''' grant_type: Right now, should always be 'client_credentials'.
            ''' </summary>
            Public grant_type As String = "client_credentials"
            ''' <summary>
            ''' client_id: Obtained from the Production Environment/Integration Environment. See https://developer.servicetitan.io/docs/get-going-manage-client-id-and-secret/
            ''' </summary>
            Public client_id As String
            ''' <summary>
            ''' client_secret: Obtained from the Production Environment/Integration Environment. See https://developer.servicetitan.io/docs/get-going-manage-client-id-and-secret/
            ''' </summary>
            Public client_secret As String
        End Class
        Public Class AccessToken
            ''' <summary>
            ''' Required to make calls to ServiceTitan Endpoints.
            ''' </summary>
            Public access_token As String
            ''' <summary>
            ''' The access_token will expire in that many seconds from the original request. See the expires_at property for the exact expiration date and time.
            ''' </summary>
            Public expires_in As Integer
            Public token_type As String
            Public scope As String
            ''' <summary>
            ''' The access_token will expire on this date and time (Local system time).
            ''' </summary>
            Public expires_at As DateTime
        End Class
        Public Shared Function getAccessToken(ByVal credentials As Credentials) As AccessToken
            Dim domain As String
            If useSandbox = True Then
                domain = "https://" & sandboxAuthEnvironment
            Else
                domain = "https://" & productionAuthEnvironment

            End If
            Dim requesturi As String = domain & "/connect/token"
            Dim args As String = "grant_type=" & credentials.grant_type
            args &= "&client_id=" & credentials.client_id
            args &= "&client_secret=" & credentials.client_secret

            Dim req As WebRequest = WebRequest.Create(requesturi)
            req.Method = "POST"
            req.Timeout = 999999
            req.ContentType = "application/x-www-form-urlencoded"
            Dim bytearray() As Byte = Encoding.UTF8.GetBytes(args)
            req.ContentLength = bytearray.Length
            Dim datastream As Stream = req.GetRequestStream
            datastream.Write(bytearray, 0, bytearray.Length)
            datastream.Close()

            Dim response As WebResponse = req.GetResponse
            Dim buffer As Stream = response.GetResponseStream
            Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
            Dim output As String = streamread.ReadToEnd
            streamread.Close()
            buffer.Close()
            response.Close()

            Dim accesstoken As AccessToken = JsonConvert.DeserializeObject(Of AccessToken)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
            accesstoken.expires_at = Now.AddSeconds(accesstoken.expires_in)
            Return accesstoken
        End Function
    End Class
    Public Class Types
        Public Class Accounting
            ''' <summary>
            ''' AdjustmentInvoiceCreateRequest: 'AdjustmentToId' is required.
            ''' </summary>
            Public Class AdjustmentInvoiceCreateRequest


                Public Property number As String
                Public Property typeId As Integer
                Public Property invoicedOn As String
                Public Property subtotal As Integer
                Public Property tax As Integer
                Public Property summary As String
                <JsonProperty("royaltyStatus")>
                Public Property royaltyStatus_ As Royaltystatus
                Public Property royaltyDate As DateTime
                Public Property royaltySentOn As String
                Public Property royaltyMemo As String
                Public Property exportId As String
                Public Property items() As New InvoiceItemUpdateRequest
                Public Property payments() As PaymentSettlementUpdateRequest
                Public Property adjustmentToId As Integer
            End Class

            Public Class Royaltystatus
                Public Property status As String
            End Class

            Public Class InvoiceItemUpdateRequest
                Public Property skuId As Integer
                Public Property skuName As String
                Public Property technicianId As Integer
                Public Property description As String
                Public Property quantity As Integer
                Public Property unitPrice As Integer
                Public Property cost As Integer
                Public Property isAddOn As Boolean
                Public Property signature As String
                Public Property technicianAcknowledgementSignature As String
                Public Property installedOn As DateTime
                Public Property inventoryWarehouseName As String
                Public Property skipUpdatingMembershipPrices As Boolean
                Public Property itemGroupName As String
                Public Property itemGroupRootId As Integer
                Public Property id As Integer
            End Class

            Public Class PaymentSettlementUpdateRequest
                Public Property id As Integer
                Public Property settlementStatus As Settlementstatus
                Public Property settlementDate As DateTime
            End Class

            Public Class Settlementstatus
                Public Property status As String
            End Class

            Public Class InvoiceResponse
                Public Property id As Integer
                Public Property syncStatus As String
                Public Property summary As String
                Public Property referenceNumber As String
                Public Property invoiceDate As DateTime
                Public Property dueDate As DateTime
                Public Property subTotal As String
                Public Property salesTax As String
                Public Property salesTaxCode As SalesTaxResponse
                Public Property total As String
                Public Property balance As String
                Public Property customer As NameFieldResponse
                Public Property customerAddress As AddressResponse
                Public Property locationAddress As AddressResponse
                Public Property businessUnit As NameFieldResponse
                Public Property termName As String
                Public Property createdBy As String
                Public Property batch As BatchResponse
                Public Property modifiedOn As DateTime
                Public Property adjustmentToId As Integer
                Public Property job As JobResponse
                Public Property projectId As Integer
                Public Property royalty As RoyaltyResponse
                Public Property employeeInfo As EmployeeInfoResponse
                Public Property commissionEligibilityDate As String
                Public Property items() As InvoiceItemResponse
                Public Property customFields() As CustomFieldResponse
            End Class

            Public Class SalesTaxResponse
                Public Property id As Integer
                Public Property name As String
                Public Property taxRate As Integer
            End Class

            Public Class NameFieldResponse
                Public Property id As Integer
                Public Property name As String
            End Class

            Public Class AddressResponse
                Public Property street As String
                Public Property unit As String
                Public Property city As String
                Public Property state As String
                Public Property zip As String
                Public Property country As String
            End Class


            Public Class BatchResponse
                Public Property id As Integer
                Public Property number As String
                Public Property name As String
            End Class

            Public Class JobResponse
                Public Property id As Integer
                Public Property number As String
                Public Property type As String
            End Class

            Public Class RoyaltyResponse
                Public Property status As String
                <JsonProperty("date")>
                Public Property _date As DateTime
                Public Property sentOn As DateTime
                Public Property memo As String
            End Class

            Public Class EmployeeInfoResponse
                Public Property id As Integer
                Public Property name As String
                Public Property modifiedOn As DateTime
            End Class

            Public Class InvoiceItemResponse
                Public Property id As Integer
                Public Property description As String
                Public Property quantity As String
                Public Property cost As String
                Public Property totalCost As String
                Public Property inventoryLocation As String
                Public Property price As String
                Public Property type As String
                Public Property skuName As String
                Public Property skuId As Integer
                Public Property total As String
                Public Property inventory As Boolean
                Public Property taxable As Boolean
                Public Property generalLedgerAccount As GLAccountResponse
                Public Property costOfSaleAccount As GLAccountResponse
                Public Property assetAccount As GLAccountResponse
                Public Property membershipTypeId As Integer
                Public Property itemGroup As ItemGroupResponse
                Public Property displayName As String
                Public Property soldHours As Integer
                Public Property modifiedOn As DateTime
                Public Property serviceDate As String
                Public Property order As Integer
            End Class

            Public Class GLAccountResponse
                Public Property name As String
                Public Property number As String
                Public Property type As String
                Public Property detailType As String
            End Class


            Public Class ItemGroupResponse
                Public Property rootId As Integer
                Public Property name As String
            End Class

            Public Class CustomFieldResponse
                Public Property name As String
                Public Property value As String
            End Class

            Public Class MarkInvoiceAsExportedUpdateRequest
                Public Property invoiceId As Integer
                Public Property externalId As String
                Public Property externalMessage As String
            End Class
            Public Class MarkInvoiceAsExportedUpdateResponse
                Public Property invoiceId As Integer
                Public Property success As Boolean
                Public Property errorMessage As String
            End Class
            Public Class InvoiceUpdateRequest
                Public Property number As String
                Public Property typeId As Integer
                Public Property invoicedOn As DateTime
                Public Property subtotal As Integer
                Public Property tax As Integer
                Public Property summary As String
                Public Property royaltyStatus As Royaltystatus
                Public Property royaltyDate As DateTime
                Public Property royaltySentOn As DateTime
                Public Property royaltyMemo As String
                Public Property exportId As String
                Public Property items() As InvoiceItemUpdateRequest
                Public Property payments() As PaymentSettlementUpdateRequest
            End Class

            Public Class CustomFieldUpdateRequest
                Public Property operations() As CustomFieldOperationRequest
            End Class

            Public Class CustomFieldOperationRequest
                Public Property objectId As Integer
                Public Property customFields() As CustomFieldPairRequest
            End Class

            Public Class CustomFieldPairRequest
                Public Property name As String
                Public Property value As String
            End Class

            Public Class PaymentCreateRequest
                Public Property typeId As Integer
                Public Property memo As String
                Public Property paidOn As DateTime
                Public Property authCode As String
                Public Property checkNumber As String
                Public Property exportId As String
                Public Property transactionStatus As PaymentStatus
                Public Property status As PaymentStatus
                Public Property splits() As PaymentSplitApiModel
            End Class

            Public Class TransactionProcessingStatus
                Public Property status As String
            End Class

            Public Class PaymentStatus
                Public Property status As String
            End Class

            Public Class PaymentSplitApiModel
                Public Property invoiceId As Integer
                Public Property amount As Integer
            End Class

            Public Class PaymentResponse
                Public Property id As Integer
                Public Property typeId As Integer
                Public Property active As Boolean
                Public Property memo As String
                Public Property paidOn As DateTime
                Public Property authCode As String
                Public Property checkNumber As String
                Public Property exportId As String
                Public Property transactionStatus As TransactionProcessingStatus
                Public Property status As PaymentStatus
                Public Property splits() As PaymentSplitApiModel
            End Class

            Public Class DetailedPaymentResponse
                Public Property id As Integer
                Public Property syncStatus As String
                Public Property referenceNumber As String
                <JsonProperty("date")>
                Public Property _date As Date
                Public Property type As String
                Public Property typeId As String
                Public Property total As String
                Public Property unappliedAmount As String
                Public Property memo As String
                Public Property customer As NamedFieldResponse
                Public Property batch As NamedFieldResponse
                Public Property createdBy As String
                Public Property generalLedgerAccount As GLAccountResponse
                Public Property appliedTo() As PaymentAppliedResponse
                Public Property customFields() As CustomFieldModel
                Public Property authCode As String
                Public Property checkNumber As String
            End Class

            Public Class NamedFieldResponse
                Public Property id As Integer
                Public Property name As String
            End Class



            Public Class PaymentAppliedResponse
                Public Property appliedTo As Integer
                Public Property appliedAmount As String
                Public Property appliedOn As DateTime
                Public Property appliedBy As String
            End Class

            Public Class CustomFieldModel
                Public Property name As String
                Public Property value As String
            End Class
            Public Class PaymentStatusBatchRequest
                Public Property status As PaymentStatus
                Public Property paymentIds() As Integer
            End Class

            Public Class PaymentUpdateRequest
                Public Property typeId As Integer
                Public Property active As Boolean
                Public Property memo As String
                Public Property paidOn As String
                Public Property authCode As String
                Public Property checkNumber As String
                Public Property exportId As String
                Public Property transactionStatus As TransactionProcessingStatus
                Public Property status As String
                Public Property splits() As PaymentSplitApiModel
            End Class

            Public Class PaymentTermModel
                Public Property id As Integer
                Public Property name As String
                Public Property dueDayType As String
                Public Property dueDay As Integer
                Public Property isCustomerDefault As Boolean
                Public Property isVendorDefault As Boolean
                Public Property active As Boolean
                Public Property inUse As Boolean
                Public Property paymentTermPenaltyModel As PaymentTermPenaltyModel
                Public Property paymentTermDiscountModel As PaymentTermDiscountModel
            End Class

            Public Class PaymentTermPenaltyModel
                Public Property id As Integer
                Public Property penaltyApplyTo As PaymentTermApplyTo
                Public Property penalty As Integer
                Public Property penaltyType As PaymentTermValueType
                Public Property maxPenaltyAmount As Integer
                Public Property penaltyFrequency As PaymentTermPenaltyFrequency
                Public Property serviceTaskId As Integer
            End Class


            Public Class PaymentTermValueType
                Public Property value As String
            End Class

            Public Class PaymentTermPenaltyFrequency
                Public Property value As String
            End Class

            Public Class PaymentTermDiscountModel
                Public Property id As Integer
                Public Property discountApplyTo As PaymentTermApplyTo
                Public Property discount As Integer
                Public Property discountType As PaymentTermValueType
                Public Property account As String
                Public Property applyBy As DiscountAppliedBy
                Public Property applyByValue As Integer
            End Class

            Public Class PaymentTermApplyTo
                Public Property value As String
            End Class

            Public Class DiscountAppliedBy
                Public Property value As String
            End Class


            Public Class PaymentTypeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As PaymentTypeResponse
                Public Property totalCount As Integer
            End Class

            Public Class PaymentTypeResponse
                Public Property id As Integer
                Public Property name As String
                Public Property modifiedOn As DateTime
            End Class

            Public Class TaxZoneResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As TaxZoneResponse
                Public Property totalCount As Integer
            End Class

            Public Class TaxZoneResponse
                Public Property id As Integer
                Public Property name As String
                Public Property color As Integer
                Public Property isTaxRateSeparated As Boolean
                Public Property isMultipleTaxZone As Boolean
                Public Property rates() As TaxRateResponse
                Public Property createdOn As DateTime
                Public Property active As Boolean
            End Class

            Public Class TaxRateResponse
                Public Property id As Integer
                Public Property taxName As String
                Public Property taxBaseType() As String
                Public Property taxRate As Integer
                Public Property salesTaxItem As String
            End Class


        End Class
        Public Class CRM

            Public Class LeadResponse
                Public Property id As Integer
                Public Property status As LeadStatus
                Public Property customerId As Integer
                Public Property locationId As Integer
                Public Property businessUnitId As Integer
                Public Property jobTypeId As Integer
                Public Property priority As Priority
                Public Property campaignId As Integer
                Public Property summary As String
                Public Property callReasonId As Integer
                Public Property latestFollowUpDate As String
                Public Property createdOn As DateTime
                Public Property createdById As Integer
                Public Property modifiedOn As DateTime
            End Class

            Public Class LeadStatus
                Public Property status As String
            End Class

            Public Class Priority
                Public Property priority As String
            End Class

            Public Class LeadResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As LeadResponse
                Public Property totalCount As Integer
            End Class

            Public Class TagsResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As TagsResponse
                Public Property totalCount As Integer
            End Class

            Public Class TagsResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
            End Class



        End Class
        Public Class Dispatch

            Public Class AppointmentAssignmentResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As AppointmentAssignmentResponse
                Public Property totalCount As Integer
            End Class

            Public Class AppointmentAssignmentResponse
                Public Property id As Integer
                Public Property technicianId As Integer
                Public Property technicianName As String
                Public Property assignedById As Integer
                Public Property assignedOn As DateTime
                Public Property status As JobAppointmentAssignmentStatus
                Public Property isPaused As Boolean
                Public Property jobId As Integer
                Public Property appointmentId As Integer
            End Class

            Public Class JobAppointmentAssignmentStatus
                Public Property status As String
            End Class

            Public Class CapacityResponse
                Public Property startsOnOrAfter As String
                Public Property endsOnOrBefore As String
                Public Property businessUnitIds() As Integer
                Public Property jobTypeId As Integer
                Public Property skillBasedAvailability As Boolean
            End Class

            Public Class TechnicianShiftResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As TechnicianShiftResponse
                Public Property totalCount As Integer
            End Class

            Public Class TechnicianShiftResponse
                Public Property id As Integer
                Public Property shiftType As Shifttype
                Public Property title As String
                Public Property note As String
                Public Property active As Boolean
                Public Property technicianId As Integer
                Public Property start As String
                <JsonProperty("end")>
                Public Property _end As DateTime
            End Class

            Public Class Shifttype
                Public Property type As String
            End Class

        End Class
        Public Class EquipmentSystems

            Public Class InstalledEquipmentCreateRequest
                Public Property locationId As Integer
                Public Property name As String
                Public Property installedOn As DateTime
                Public Property serialNumber As String
                Public Property memo As String
                Public Property manufacturer As String
                Public Property model As String
                Public Property cost As Integer
                Public Property manufacturerWarrantyStart As DateTime
                Public Property manufacturerWarrantyEnd As DateTime
                Public Property serviceProviderWarrantyStart As DateTime
                Public Property serviceProviderWarrantyEnd As DateTime
                Public Property customFields() As CustomFieldRequestModel
            End Class

            Public Class CustomFieldRequestModel
                Public Property id As Integer
                Public Property typeId As Integer
                Public Property value As String
            End Class

            Public Class InstalledEquipmentDetailedResponse
                Public Property id As Integer
                Public Property locationId As Integer
                Public Property customerId As Integer
                Public Property name As String
                Public Property installedOn As DateTime
                Public Property serialNumber As String
                Public Property memo As String
                Public Property manufacturer As String
                Public Property model As String
                Public Property cost As Integer
                Public Property manufacturerWarrantyStart As DateTime
                Public Property manufacturerWarrantyEnd As DateTime
                Public Property serviceProviderWarrantyStart As DateTime
                Public Property serviceProviderWarrantyEnd As DateTime
                Public Property customFields() As CustomFieldResponseModel
            End Class

            Public Class CustomFieldResponseModel
                Public Property id As Integer
                Public Property typeId As Integer
                Public Property name As String
                Public Property value As String
            End Class

            Public Class InstalledEquipmentResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As InstalledEquipmentResponse
                Public Property totalCount As Integer
            End Class

            Public Class InstalledEquipmentResponse
                Public Property id As Integer
                Public Property locationId As Integer
                Public Property customerId As Integer
                Public Property name As String
                Public Property installedOn As DateTime
                Public Property serialNumber As String
                Public Property memo As String
                Public Property manufacturer As String
                Public Property model As String
                Public Property cost As Integer
                Public Property manufacturerWarrantyStart As DateTime
                Public Property manufacturerWarrantyEnd As DateTime
                Public Property serviceProviderWarrantyStart As DateTime
                Public Property serviceProviderWarrantyEnd As DateTime
            End Class

            Public Class InstalledEquipmentUpdateRequest
                Public Property name As String
                Public Property installedOn As DateTime
                Public Property serialNumber As String
                Public Property memo As String
                Public Property manufacturer As String
                Public Property model As String
                Public Property cost As Integer
                Public Property manufacturerWarrantyStart As DateTime
                Public Property manufacturerWarrantyEnd As DateTime
                Public Property serviceProviderWarrantyStart As DateTime
                Public Property serviceProviderWarrantyEnd As DateTime
                Public Property customFields() As CustomFieldRequestModel
            End Class



        End Class
        Public Class Inventory

            Public Class CreatePurchaseOrderRequest
                Public Property vendorId As Integer
                Public Property typeId As Integer
                Public Property businessUnitId As Integer
                Public Property inventoryLocationId As Integer
                Public Property jobId As Integer
                Public Property technicianId As Integer
                Public Property projectId As Integer
                Public Property shipTo As CreateAddressRequest
                Public Property vendorInvoiceNumber As String
                Public Property impactsTechnicianPayroll As Boolean
                Public Property memo As String
                <JsonProperty("date")>
                Public Property _date As DateTime
                Public Property requiredOn As DateTime
                Public Property tax As Integer
                Public Property shipping As Integer
                Public Property items() As CreatePurchaseOrderItemRequest
            End Class

            Public Class CreateAddressRequest
                Public Property description As String
                Public Property address As AddressRequest
            End Class

            Public Class AddressRequest
                Public Property street As String
                Public Property unit As String
                Public Property city As String
                Public Property state As String
                Public Property zip As String
                Public Property country As String
            End Class

            Public Class CreatePurchaseOrderItemRequest
                Public Property skuId As Integer
                Public Property description As String
                Public Property vendorPartNumber As String
                Public Property quantity As Integer
                Public Property cost As Integer
            End Class

            Public Class PurchaseOrderResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As PurchaseOrderResponse
                Public Property totalCount As Integer
            End Class

            Public Class PurchaseOrderResponse
                Public Property id As Integer
                Public Property number As String
                Public Property invoiceId As Integer
                Public Property jobId As Integer
                Public Property projectId As Integer
                Public Property status As String
                Public Property typeId As Integer
                Public Property vendorId As Integer
                Public Property technicianId As Integer
                Public Property shipTo As AddressResponse
                Public Property businessUnitId As Integer
                Public Property inventoryLocationId As Integer
                Public Property batchId As Integer
                Public Property vendorDocumentNumber As String
                <JsonProperty("date")>
                Public Property _date As String
                Public Property requiredOn As DateTime
                Public Property sentOn As DateTime
                Public Property receivedOn As DateTime
                Public Property modifiedOn As DateTime
                Public Property total As Integer
                Public Property tax As Integer
                Public Property shipping As Integer
                Public Property summary As String
                Public Property items() As PurchaseOrderItemResponse
                Public Property customFields() As CustomFieldApiModel
            End Class

            Public Class AddressResponse
                Public Property street As String
                Public Property unit As String
                Public Property city As String
                Public Property state As String
                Public Property zip As String
                Public Property country As String
            End Class

            Public Class PurchaseOrderItemResponse
                Public Property id As Integer
                Public Property skuId As Integer
                Public Property skuName As String
                Public Property skuCode As String
                Public Property skuType As String
                Public Property description As String
                Public Property vendorPartNumber As String
                Public Property quantity As Integer
                Public Property quantityReceived As Integer
                Public Property cost As Integer
                Public Property total As Integer
                Public Property serialNumbers() As SerialNumberResponse
                Public Property status As String
                Public Property chargeable As Boolean
            End Class

            Public Class SerialNumberResponse
                Public Property id As Integer
                Public Property number As String
            End Class

            Public Class CustomFieldApiModel
                Public Property typeId As Integer
                Public Property name As String
                Public Property value As String
            End Class

            Public Class UpdatePurchaseOrderRequest
                Public Property vendorId As Integer
                Public Property typeId As Integer
                Public Property businessUnitId As Integer
                Public Property inventoryLocationId As Integer
                Public Property jobId As Integer
                Public Property technicianId As Integer
                Public Property projectId As Integer
                Public Property shipTo As UpdateAddressRequest
                Public Property vendorInvoiceNumber As String
                Public Property impactsTechnicianPayroll As Boolean
                Public Property memo As String
                <JsonProperty("date")>
                Public Property _date As DateTime
                Public Property requiredOn As DateTime
                Public Property tax As Integer
                Public Property shipping As Integer
                Public Property items() As UpdatePurchaseOrderItemRequest
                Public Property removedItems() As RemovePurchaseOrderItemRequest
            End Class

            Public Class UpdateAddressRequest
                Public Property description As String
                Public Property address As AddressRequest
            End Class

            Public Class UpdatePurchaseOrderItemRequest
                Public Property id As Integer
                Public Property skuId As Integer
                Public Property description As String
                Public Property vendorPartNumber As String
                Public Property quantity As Integer
                Public Property cost As Integer
            End Class

            Public Class RemovePurchaseOrderItemRequest
                Public Property id As Integer
                Public Property doNotReplenish As Boolean
            End Class

            Public Class VendorResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As VendorResponse
                Public Property totalCount As Integer
            End Class

            Public Class VendorResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
                Public Property isTruckReplenishment As Boolean
                Public Property isMobileCreationRestricted As Boolean
                Public Property memo As String
                Public Property deliveryOption As String
                Public Property defaultTaxRate As Integer
                Public Property contactInfo As VendorContactInfoResponse
                Public Property address As AddressResponse
            End Class

            Public Class VendorContactInfoResponse
                Public Property firstName As String
                Public Property lastName As String
                Public Property phone As String
                Public Property email As String
                Public Property fax As String
            End Class

        End Class
        Public Class JobBooking_ContactExperience

            Public Class CallReasonResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As CallReasonResponse
                Public Property totalCount As Integer
            End Class

            Public Class CallReasonResponse
                Public Property id As Integer
                Public Property name As String
                Public Property isLead As Boolean
                Public Property active As Boolean
            End Class

        End Class
        Public Class JobPlanning_Management

            Public Class AppointmentAddRequest
                Public Property jobId As Integer
                Public Property start As DateTime
                <JsonProperty("end")>
                Public Property _end As DateTime
                Public Property arrivalWindowStart As DateTime
                Public Property arrivalWindowEnd As DateTime
                Public Property technicianIds() As Integer
                Public Property specialInstructions As String
            End Class

            Public Class AppointmentResponse
                Public Property id As Integer
                Public Property jobId As Integer
                Public Property appointmentNumber As String
                Public Property start As String
                Public Property _end As String
                Public Property arrivalWindowStart As DateTime
                Public Property arrivalWindowEnd As DateTime
                Public Property status As Status
                Public Property specialInstructions As String
                Public Property createdOn As DateTime
                Public Property modifiedOn As DateTime
            End Class

            Public Class Status
                Public Property status As String
            End Class

            Public Class UpdateAppointmentSpecialInstructionsRequest
                Public Property specialInstructions As String
            End Class

            Public Class AppointmentResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As AppointmentResponse
                Public Property totalCount As Integer
            End Class

            Public Class HoldAppointmentRequest
                Public Property reasonId As Integer
                Public Property memo As String
            End Class

            Public Class AppointmentRescheduleRequest
                Public Property start As DateTime
                <JsonProperty("end")>
                Public Property _end As DateTime
                Public Property arrivalWindowStart As DateTime
                Public Property arrivalWindowEnd As DateTime
            End Class

            Public Class JobCancelReasonResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As JobCancelReasonResponse
                Public Property totalCount As Integer
            End Class

            Public Class JobCancelReasonResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
            End Class

            Public Class JobHoldReasonResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As JobHoldReasonResponse
                Public Property totalCount As Integer
            End Class

            Public Class JobHoldReasonResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
            End Class

            Public Class CancelJobRequest
                Public Property reasonId As Integer
                Public Property memo As String
            End Class

            Public Class JobCreateRequest
                Public Property customerId As Integer
                Public Property locationId As Integer
                Public Property businessUnitId As Integer
                Public Property jobTypeId As Integer
                Public Property priority As String
                Public Property campaignId As Integer
                Public Property appointments() As AppointmentInformation
                Public Property summary As String
                Public Property customFields() As CustomFieldApiModel
                Public Property tagTypeIds() As Integer
                Public Property externalData As ExternalDataUpdateRequest
            End Class

            Public Class ExternalDataUpdateRequest
                Public Property applicationGuid As String
                Public Property externalData() As ExternalDataModel
            End Class

            Public Class ExternalDataModel
                Public Property key As String
                Public Property value As String
            End Class

            Public Class AppointmentInformation
                Public Property start As DateTime
                <JsonProperty("end")>
                Public Property _end As DateTime
                Public Property arrivalWindowStart As DateTime
                Public Property arrivalWindowEnd As DateTime
                Public Property technicianIds() As Integer
            End Class

            Public Class CustomFieldApiModel
                Public Property typeId As Integer
                Public Property name As String
                Public Property value As String
            End Class

            Public Class JobResponse
                Public Property id As Integer
                Public Property jobNumber As String
                Public Property customerId As Integer
                Public Property locationId As Integer
                Public Property jobStatus As String
                Public Property completedOn As DateTime
                Public Property businessUnitId As Integer
                Public Property jobTypeId As Integer
                Public Property priority As String
                Public Property campaignId As Integer
                Public Property summary As String
                Public Property customFields() As CustomFieldApiModel
                Public Property appointmentCount As Integer
                Public Property firstAppointmentId As Integer
                Public Property lastAppointmentId As Integer
                Public Property recallForId As Integer
                Public Property warrantyId As Integer
                Public Property jobGeneratedLeadSource As JobGeneratedLeadSource
                Public Property noCharge As Boolean
                Public Property notificationsEnabled As Boolean
                Public Property createdOn As DateTime
                Public Property createdById As Integer
                Public Property modifiedOn As DateTime
                Public Property tagTypeIds() As Integer
                Public Property leadCallId As Integer
                Public Property bookingId As Integer
                Public Property soldById As Integer
                Public Property externalData() As ExternalDataUpdateRequest
            End Class

            Public Class JobGeneratedLeadSource
                Public Property jobId As Integer
                Public Property employeeId As Integer
            End Class

            Public Class JobNoteCreateRequest
                Public Property text As String
                Public Property pinToTop As Boolean
            End Class

            Public Class NoteResponse
                Public Property text As String
                Public Property isPinned As Boolean
                Public Property createdById As Integer
                Public Property createdOn As DateTime
            End Class

            Public Class CancelReasonResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As CancelReasonResponse
                Public Property totalCount As Integer
            End Class

            Public Class CancelReasonResponse
                Public Property jobId As Integer
                Public Property reasonId As Integer
                Public Property name As String
                Public Property text As String
            End Class

            Public Class JobHistoryResponse
                Public Property history() As JobHistoryItemModel
            End Class

            Public Class JobHistoryItemModel
                Public Property id As Integer
                Public Property employeeId As Integer
                Public Property eventType As String
                <JsonProperty("date")>
                Public Property _date As DateTime
                Public Property usedSchedulingTool As String
            End Class

            Public Class JobResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As JobResponse
                Public Property totalCount As Integer
            End Class

            Public Class NoteResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As NoteResponse
                Public Property totalCount As Integer
            End Class

            Public Class HoldJobRequest
                Public Property reasonId As Integer
                Public Property memo As String
            End Class

            Public Class UpdateJobRequest
                Public Property customerId As Integer
                Public Property locationId As Integer
                Public Property businessUnitId As Integer
                Public Property jobTypeId As Integer
                Public Property priority As String
                Public Property campaignId As Integer
                Public Property summary As String
                Public Property shouldUpdateInvoiceItems As Boolean
                Public Property customFields() As CustomFieldApiModel
                Public Property tagIds() As Integer
                Public Property externalData As ExternalDataUpdateRequest
            End Class

            Public Class UpdateJobTypeRequest
                Public Property name As String
                Public Property externalData As ExternalDataUpdateRequest
            End Class

            Public Class CreateJobTypeRequest
                Public Property name As String
                Public Property externalData As ExternalDataUpdateRequest
            End Class

            Public Class JobTypeResponse
                Public Property id As Integer
                Public Property name As String
                Public Property modifiedOn As DateTime
                Public Property externalData() As ExternalDataUpdateRequest
            End Class

            Public Class JobTypeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As JobTypeResponse
                Public Property totalCount As Integer
            End Class

            Public Class ProjectResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As ProjectResponse
                Public Property totalCount As Integer
            End Class

            Public Class ProjectResponse
                Public Property id As Integer
                Public Property number As String
                Public Property name As String
                Public Property summary As String
                Public Property customerId As Integer
                Public Property locationId As Integer
                Public Property startDate As DateTime
                Public Property targetCompletionDate As DateTime
                Public Property actualCompletionDate As DateTime
                Public Property customFields() As CustomFieldApiModel
            End Class















        End Class
        Public Class Marketing

            Public Class CreateCostRequest
                Public Property campaignId As Integer
                Public Property year As Integer
                Public Property month As Integer
                Public Property dailyCost As Integer
            End Class

            Public Class CampaignCostModel
                Public Property id As Integer
                Public Property year As Integer
                Public Property month As Integer
                Public Property dailyCost As Integer
            End Class

            Public Class UpdateCostRequest
                Public Property id As Integer
                Public Property dailyCost As Integer
            End Class

            Public Class CampaignCreateModel
                Public Property name As String
                Public Property businessUnitId As Integer
                Public Property dnis As String
                Public Property cost As Integer
                Public Property categoryId As Integer
                Public Property active As Boolean
            End Class

            Public Class DetailedCampaignModel
                Public Property id As Integer
                Public Property name As String
                Public Property modifiedOn As DateTime
                Public Property active As Boolean
                Public Property category As DetailedCampaignCategoryModel
            End Class

            Public Class DetailedCampaignCategoryModel
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
            End Class

            Public Class CampaignModel_P
                Public Property data() As CampaignModel
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property totalCount As Integer
                Public Property hasMore As Boolean
            End Class

            Public Class CampaignModel
                Public Property id As Integer
                Public Property name As String
                Public Property modifiedOn As DateTime
                Public Property active As Boolean
                Public Property category As CampaignCategoryModel
            End Class

            Public Class CampaignCategoryModel
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
            End Class

            Public Class DetailedCampaignModel_R
                Public Property data As DetailedCampaignModel
            End Class







        End Class
        Public Class Memberships

            Public Class MembershipSaleInvoiceCreateRequest
                Public Property customerId As Integer
                Public Property businessUnitId As Integer
                Public Property saleTaskId As Integer
                Public Property durationBillingId As Integer
                Public Property locationId As Integer
                Public Property recurringServiceAction As String
                Public Property recurringLocationId As Integer
            End Class

            Public Class MembershipSaleInvoiceCreateResponse
                Public Property invoiceId As Integer
                Public Property customerMembershipId As Integer
            End Class

            Public Class CustomerMembershipResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As CustomerMembershipResponse
                Public Property totalCount As Integer
            End Class

            Public Class CustomerMembershipResponse
                Public Property id As Integer
                Public Property followUpOn As DateTime
                Public Property modifiedOn As DateTime
                Public Property cancellationDate As DateTime
                Public Property createdOn As DateTime
                Public Property from As DateTime
                Public Property nextScheduledBillDate As DateTime
                <JsonProperty("to")>
                Public Property _to As DateTime
                Public Property billingFrequency As MembershipRecurrenceType
                Public Property renewalBillingFrequency As MembershipRecurrenceType
                Public Property status As MembershipStatus
                Public Property followUpStatus As OpportunityStatus
                Public Property active As Boolean
                Public Property initialDeferredRevenue As Integer
                Public Property duration As Integer
                Public Property renewalDuration As Integer
                Public Property businessUnitId As Integer
                Public Property customerId As Integer
                Public Property membershipTypeId As Integer
                Public Property activatedById As Integer
                Public Property activatedFromId As Integer
                Public Property billingTemplateId As Integer
                Public Property cancellationBalanceInvoiceId As Integer
                Public Property cancellationInvoiceId As Integer
                Public Property createdById As Integer
                Public Property followUpCustomStatusId As Integer
                Public Property locationId As Integer
                Public Property paymentMethodId As Integer
                Public Property paymentTypeId As Integer
                Public Property recurringLocationId As Integer
                Public Property renewalMembershipTaskId As Integer
                Public Property renewedById As Integer
                Public Property soldById As Integer
                Public Property customerPo As String
                Public Property importId As String
                Public Property memo As String
            End Class

            Public Class MembershipRecurrenceType
                Public Property RecurrenceType As String
            End Class


            Public Class MembershipStatus
                Public Property status As String
            End Class

            Public Class OpportunityStatus
                Public Property status As String
            End Class

            Public Class CustomerMembershipUpdateRequest
                Public Property businessUnitId As Integer
                Public Property nextScheduledBillDate As DateTime
                Public Property status As MembershipStatus
                Public Property memo As String
                Public Property from As DateTime
                <JsonProperty("to")>
                Public Property _to As DateTime
                Public Property soldById As Integer
                Public Property billingTemplateId As Integer
                Public Property locationId As Integer
                Public Property recurringServiceAction As RecurringServiceAction
                Public Property recurringLocationId As Integer
                Public Property paymentMethodId As Integer
                Public Property paymentTypeId As Integer
                Public Property renewalMembershipTaskId As Integer
                Public Property initialDeferredRevenue As Integer
                Public Property cancellationBalanceInvoiceId As Integer
                Public Property cancellationInvoiceId As Integer
                Public Property active As Boolean
                Public Property duration As Integer
            End Class



            Public Class RecurringServiceAction
                Public Property action As String
            End Class

            Public Class InvoiceTemplateCreateRequest
                Public Property name As String
                Public Property items() As InvoiceTemplateItemCreateRequest
            End Class

            Public Class InvoiceTemplateItemCreateRequest
                Public Property skuId As Integer
                Public Property quantity As Integer
                Public Property unitPrice As Integer
                Public Property isAddOn As Boolean
                Public Property workflowActionItemId As Integer
                Public Property description As String
                Public Property cost As Integer
                Public Property hours As Integer
            End Class
            Public Class ModificationResponse
                Public Property id As Integer
            End Class

            Public Class InvoiceTemplateResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As InvoiceTemplateResponse
                Public Property totalCount As Integer
            End Class

            Public Class InvoiceTemplateResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
                Public Property total As Integer
                Public Property isSettingsTemplate As Boolean
                Public Property importId As String
                Public Property items() As InvoiceTemplateItemResponse
            End Class

            Public Class InvoiceTemplateItemResponse
                Public Property id As Integer
                Public Property skuId As Integer
                Public Property skuType As SkuType
                Public Property quantity As Integer
                Public Property unitPrice As Integer
                Public Property isAddOn As Boolean
                Public Property importId As String
                Public Property workflowActionItemId As Integer
                Public Property description As String
                Public Property cost As Integer
                Public Property hours As Integer
            End Class

            Public Class SkuType
                Public Property type As String
            End Class

            Public Class InvoiceTemplateUpdateRequest
                Public Property name As String
                Public Property createdOn As DateTime
                Public Property createdById As Integer
                Public Property active As Boolean
                Public Property items() As InvoiceTemplateItemUpdateRequest
            End Class

            Public Class InvoiceTemplateItemUpdateRequest
                Public Property id As Integer
                Public Property skuId As Integer
                Public Property quantity As Integer
                Public Property unitPrice As Integer
                Public Property isAddOn As Boolean
                Public Property description As String
                Public Property cost As Integer
                Public Property hours As Integer
            End Class

            Public Class LocationRecurringServiceEventResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As LocationRecurringServiceEventResponse
                Public Property totalCount As Integer
            End Class

            Public Class LocationRecurringServiceEventResponse
                Public Property id As Integer
                Public Property locationRecurringServiceId As Integer
                Public Property locationRecurringServiceName As String
                Public Property membershipId As Integer
                Public Property membershipName As String
                Public Property status As OpportunityStatus
                <JsonProperty("date")>
                Public Property _date As DateTime
                Public Property createdOn As DateTime
            End Class

            Public Class MarkEventCompletedStatusUpdateRequest
                Public Property jobId As Integer
            End Class

            Public Class LocationRecurringServiceResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As LocationRecurringServiceResponse
                Public Property totalCount As Integer
            End Class

            Public Class LocationRecurringServiceResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
                Public Property createdOn As DateTime
                Public Property createdById As Integer
                Public Property modifiedOn As DateTime
                Public Property importId As String
                Public Property membershipId As Integer
                Public Property locationId As Integer
                Public Property recurringServiceTypeId As Integer
                Public Property durationType As ServiceRecurrenceDuration
                Public Property durationLength As Integer
                Public Property from As DateTime
                <JsonProperty("to")>
                Public Property _to As DateTime
                Public Property memo As String
                Public Property invoiceTemplateId As Integer
                Public Property invoiceTemplateForFollowingYearsId As Integer
                Public Property firstVisitComplete As Boolean
                Public Property activatedFromId As Integer
                Public Property allocation As Integer
                Public Property businessUnitId As Integer
                Public Property jobTypeId As Integer
                Public Property campaignId As Integer
                Public Property priority As Priority
                Public Property jobSummary As String
                Public Property recurrenceType As ServiceRecurrenceType
                Public Property recurrenceInterval As Integer
                Public Property recurrenceMonths() As String
                Public Property recurrenceDaysOfWeek() As String
                Public Property recurrenceWeek As WeekDay
                Public Property recurrenceDayOfNthWeek As DayOfWeek
                Public Property recurrenceDaysOfMonth() As Integer
                Public Property jobStartTime As String
                Public Property estimatedPayrollCost As Integer
            End Class

            Public Class ServiceRecurrenceDuration
                Public Property duration As String
            End Class

            Public Class Priority
                Public Property priority As String
            End Class

            Public Class ServiceRecurrenceType
                Public Property type As String
            End Class

            Public Class WeekDay
                Public Property WeekDay As String
            End Class

            Public Class DayOfWeek
                Public Property DayOfWeek As String
            End Class

            Public Class LocationRecurringServiceUpdateRequest
                Public Property name As String
                Public Property active As Boolean
                Public Property recurringServiceTypeId As Integer
                Public Property durationType As String
                Public Property durationLength As Integer
                Public Property from As DateTime
                Public Property memo As String
                Public Property invoiceTemplateId As Integer
                Public Property invoiceTemplateForFollowingYearsId As Integer
                Public Property businessUnitId As Integer
                Public Property jobTypeId As Integer
                Public Property campaignId As Integer
                Public Property priority As String
                Public Property jobSummary As String
                Public Property recurrenceType As String
                Public Property recurrenceInterval As Integer
                Public Property recurrenceMonths() As String
                Public Property recurrenceDaysOfWeek() As String
                Public Property recurrenceWeek As String
                Public Property recurrenceDayOfNthWeek As DayOfWeek
                Public Property jobStartTime As String
                Public Property estimatedPayrollCost As Integer
            End Class

            Public Class MembershipTypeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As MembershipTypeResponse
                Public Property totalCount As Integer
            End Class

            Public Class MembershipTypeResponse
                Public Property name As String
                Public Property active As Boolean
                Public Property discountMode As DiscountMode
                Public Property locationTarget As MembershipLocationTarget
                Public Property revenueRecognitionMode As RevenueRecognitionMode
                Public Property autoCalculateInvoiceTemplates As Boolean
                Public Property useMembershipPricingTable As Boolean
                Public Property showMembershipSavings As Boolean
                Public Property id As Integer
                Public Property createdOn As DateTime
                Public Property createdById As Integer
                Public Property modifiedOn As DateTime
                Public Property importId As String
                Public Property billingTemplateId As Integer
            End Class

            Public Class DiscountMode
                Public Property mode As String
            End Class

            Public Class MembershipLocationTarget
                Public Property target As String
            End Class

            Public Class RevenueRecognitionMode
                Public Property mode As String
            End Class



            Public Class MembershipTypeDiscountItemResponse
                Public Property id As Integer
                Public Property targetId As Integer
                Public Property discount As Integer
            End Class

            Public Class MembershipTypeDurationBillingItemResponse
                Public Property id As Integer
                Public Property duration As Integer
                Public Property billingFrequency As MembershipRecurrenceType
                Public Property salePrice As Integer
                Public Property billingPrice As Integer
                Public Property renewalPrice As Integer
                Public Property importId As String
                Public Property active As Boolean
            End Class



            Public Class MembershipTypeRecurringServiceItemResponse
                Public Property id As Integer
                Public Property membershipTypeId As Integer
                Public Property recurringServiceTypeId As Integer
                Public Property offset As Integer
                Public Property offsetType As OffsetType
                Public Property allocation As Integer
                Public Property importId As String
            End Class

            Public Class OffsetType
                Public Property type As String
            End Class

            Public Class RecurringServiceTypeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As RecurringServiceTypeResponse
                Public Property totalCount As Integer
            End Class

            Public Class RecurringServiceTypeResponse
                Public Property id As Integer
                Public Property active As Boolean
                Public Property recurrenceType As ServiceRecurrenceType
                Public Property recurrenceInterval As Integer
                Public Property recurrenceMonths() As String
                Public Property durationType As ServiceRecurrenceDuration
                Public Property durationLength As Integer
                Public Property invoiceTemplateId As Integer
                Public Property businessUnitId As Integer
                Public Property jobTypeId As Integer
                Public Property priority As Priority
                Public Property campaignId As Integer
                Public Property jobSummary As String
                Public Property name As String
                Public Property importId As String
            End Class









        End Class
        Public Class Payroll

            Public Class PayrollActivityCodeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As PayrollActivityCodeResponse
                Public Property totalCount As Integer
            End Class

            Public Class PayrollActivityCodeResponse
                Public Property id As Integer
                Public Property name As String
                Public Property code As String
                Public Property earningCategory As PayrollEarningCategory
            End Class

            Public Class PayrollEarningCategory
                Public Property category As String
            End Class

            Public Class GrossPayItemCreateRequest
                Public Property payrollId As Integer
                Public Property amount As Integer
                Public Property activityCodeId As Integer
                <JsonProperty("date")>
                Public Property _date As DateTime
                Public Property invoiceId As Integer
            End Class

            Public Class GrossPayItemResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As GrossPayItemResponse
                Public Property totalCount As Integer
            End Class

            Public Class GrossPayItemResponse
                Public Property id As Integer
                Public Property employeeId As Integer
                Public Property employeeType As EmployeeType
                Public Property businessUnitName As String
                Public Property payrollId As Integer
                <JsonProperty("date")>
                Public Property _date As DateTime
                Public Property activity As String
                Public Property activityCodeId As Integer
                Public Property activityCode As String
                Public Property amount As Integer
                Public Property amountAdjustment As Integer
                Public Property payoutBusinessUnitName As String
                Public Property grossPayItemType As GrossPayItemType
                Public Property startTime As String
                Public Property endTime As String
                Public Property paidDurationHours As Integer
                Public Property paidTimeType As PaidTimeType
                Public Property jobId As Integer
                Public Property jobNumber As String
                Public Property jobTypeName As String
                Public Property projectNumber As String
                Public Property projectId As Integer
                Public Property invoiceId As Integer
                Public Property invoiceNumber As String
                Public Property invoiceItemId As Integer
                Public Property customerId As Integer
                Public Property customerName As String
                Public Property locationId As Integer
                Public Property locationName As String
                Public Property locationAddress As String
                Public Property locationZip As String
                Public Property zoneName As String
                Public Property taxZoneName As String
                Public Property laborTypeId As Integer
                Public Property laborTypeCode As String
                Public Property isPrevailingWageJob As Boolean
            End Class

            Public Class EmployeeType
                Public Property type As String
            End Class

            Public Class GrossPayItemType
                Public Property type As String

            End Class

            Public Class PaidTimeType
                Public Property type As String

            End Class

            Public Class GrossPayItemUpdateRequest
                Public Property payrollId As Integer
                Public Property amount As Integer
                Public Property activityCodeId As Integer
                <JsonProperty("date")>
                Public Property _date As DateTime
                Public Property invoiceId As Integer
            End Class

            Public Class JobSplitResponse
                Public Property jobId As Integer
                Public Property technicianId As Integer
                Public Property split As Integer
            End Class

            Public Class PayrollAdjustmentCreateRequest
                Public Property employeeType As EmployeeType
                Public Property employeeId As Integer
                Public Property postedOn As DateTime
                Public Property amount As Integer
                Public Property memo As String
                Public Property activityCodeId As Integer
                Public Property hours As Integer
                Public Property rate As Integer
            End Class

            Public Class PayrollAdjustmentResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As PayrollAdjustmentResponse
                Public Property totalCount As Integer
            End Class

            Public Class PayrollAdjustmentResponse
                Public Property id As Integer
                Public Property employeeType As EmployeeType
                Public Property employeeId As Integer
                Public Property postedOn As DateTime
                Public Property amount As Integer
                Public Property memo As String
                Public Property activityCodeId As Integer
                Public Property hours As Integer
                Public Property rate As Integer
            End Class

            Public Class PayrollResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As PayrollResponse
                Public Property totalCount As Integer
            End Class

            Public Class PayrollResponse
                Public Property id As Integer
                Public Property fromDate As Date
                Public Property toDate As Date
                Public Property employeeId As Integer
                Public Property employeeType As EmployeeType
                Public Property status As PayrollStatus
            End Class



            Public Class PayrollStatus
                Public Property status As String
            End Class


            Public Class TimesheetCodeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As TimesheetCodeResponse
                Public Property totalCount As Integer
            End Class

            Public Class TimesheetCodeResponse
                Public Property id As Integer
                Public Property code As String
                Public Property description As String
                Public Property type As TimesheetCodeType
                Public Property applicableEmployeeType As TimesheetCodeEmployeeType
                Public Property rateInfo As TimesheetCodeRateInfoResponse
            End Class

            Public Class TimesheetCodeType
                Public Property type As String
            End Class

            Public Class TimesheetCodeEmployeeType
                Public Property type As String

            End Class

            Public Class TimesheetCodeRateInfoResponse
                Public Property hourlyRate As TimesheetHourlyRateType
                Public Property customHourlyRate As Integer
                Public Property rateMultiplier As Integer
            End Class

            Public Class TimesheetHourlyRateType
                Public Property type As String

            End Class



        End Class
        Public Class Pricebook
            'TODO: Category_Get and Category_GetList
            Public Class CategoryCreateRequest
                Public Property name As String
                Public Property active As Boolean
                Public Property description As String
                Public Property parentId As Integer
                Public Property position As Integer
                Public Property image As String
                Public Property categoryType As String
                Public Property businessUnitIds() As Integer
                Public Property skuImages() As String
                Public Property skuVideos() As String
            End Class

            Public Class CategoryUpdateRequest
                Public Property name As String
                Public Property active As Boolean
                Public Property description As String
                Public Property parentId As Integer
                Public Property position As Integer
                Public Property image As String
                Public Property categoryType As String
                Public Property businessUnitIds() As Integer
                Public Property skuImages() As String
                Public Property skuVideos() As String
            End Class

            Public Class DiscountAndFeesCreateRequest
                Public Property type As String
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property amountType As String
                Public Property amount As Integer
                Public Property limit As Integer
                Public Property taxable As Boolean
                Public Property categories() As Integer
                Public Property hours As Integer
                Public Property assets() As SkuAssetResponse
                Public Property account As String
                Public Property crossSaleGroup As String
                Public Property active As Boolean
                Public Property bonus As Integer
                Public Property commissionBonus As Integer
                Public Property paysCommission As Boolean
                Public Property excludeFromPayroll As Boolean
            End Class

            Public Class SkuAssetResponse
                Public Property type As String
                <JsonProperty("alias")>
                Public Property _alias As String
                Public Property url As String
            End Class

            Public Class DiscountAndFeesResponse
                Public Property id As Integer
                Public Property type As String
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property amountType As String
                Public Property amount As Integer
                Public Property limit As Integer
                Public Property taxable As Boolean
                Public Property categories() As Integer
                Public Property hours As Integer
                Public Property assets() As SkuAssetResponse
                Public Property account As String
                Public Property crossSaleGroup As String
                Public Property active As Boolean
                Public Property bonus As Integer
                Public Property commissionBonus As Integer
                Public Property paysCommission As Boolean
                Public Property excludeFromPayroll As Boolean
            End Class

            Public Class DiscountAndFeesResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As DiscountAndFeesResponse
                Public Property totalCount As Integer
            End Class

            Public Class DiscountAndFeesUpdateRequest
                Public Property type As String
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property amountType As String
                Public Property amount As Integer
                Public Property limit As Integer
                Public Property taxable As Boolean
                Public Property categories() As Integer
                Public Property hours As Integer
                Public Property assets() As SkuAssetRequest
                Public Property account As String
                Public Property crossSaleGroup As String
                Public Property active As Boolean
                Public Property bonus As Integer
                Public Property commissionBonus As Integer
                Public Property paysCommission As Boolean
                Public Property excludeFromPayroll As Boolean
            End Class

            Public Class SkuAssetRequest
                Public Property type As String
                <JsonProperty("alias")>
                Public Property _alias As String
                Public Property url As String
            End Class

            Public Class EquipmentCreateRequest
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property active As Boolean
                Public Property manufacturer As String
                Public Property model As String
                Public Property manufacturerWarranty As SkuWarrantyRequest
                Public Property serviceProviderWarranty As SkuWarrantyRequest
                Public Property assets() As SkuAssetRequest
                Public Property recommendations() As EquipmentRecommendationResponse
                Public Property upgrades() As Integer
                Public Property equipmentMaterials() As SkuLinkResponse
                Public Property categories() As Integer
                Public Property primaryVendor As SkuVendorResponse
                Public Property otherVendors() As SkuVendorResponse
                Public Property account As String
                Public Property costOfSaleAccount As String
                Public Property assetAccount As String
                Public Property crossSaleGroup As String
                Public Property paysCommission As Boolean
                Public Property commissionBonus As Integer
                Public Property hours As Integer
                Public Property taxable As Boolean
                Public Property cost As Integer
                Public Property unitOfMeasure As String
                Public Property isInventory As Boolean
            End Class

            Public Class SkuWarrantyRequest
                Public Property duration As Integer
                Public Property description As String
            End Class


            Public Class SkuVendorResponse
                Public Property vendorId As Integer
                Public Property memo As String
                Public Property vendorPart As String
                Public Property cost As Integer
                Public Property active As Boolean
                Public Property primarySubAccount As SkuVendorSubAccountResponse
                Public Property otherSubAccounts() As SkuVendorSubAccountResponse
            End Class


            Public Class SkuVendorSubAccountResponse
                Public Property cost As Integer
                Public Property accountName As String
            End Class

            Public Class EquipmentRecommendationResponse
                Public Property skuId As Integer
                Public Property type As String
            End Class

            Public Class SkuLinkResponse
                Public Property skuId As Integer
                Public Property quantity As Integer
            End Class

            Public Class EquipmentResponse
                Public Property id As Integer
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property active As Boolean
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property manufacturer As String
                Public Property model As String
                Public Property manufacturerWarranty As SkuWarrantyResponse
                Public Property serviceProviderWarranty As SkuWarrantyResponse
                Public Property categories() As Integer
                Public Property assets() As SkuAssetResponse
                Public Property recommendations() As EquipmentRecommendationResponse
                Public Property upgrades() As Integer
                Public Property equipmentMaterials() As SkuLinkResponse
                Public Property primaryVendor As SkuVendorResponse
                Public Property otherVendors() As SkuVendorResponse
                Public Property account As String
                Public Property costOfSaleAccount As String
                Public Property assetAccount As String
                Public Property crossSaleGroup As String
                Public Property paysCommission As Boolean
                Public Property commissionBonus As Integer
                Public Property hours As Integer
                Public Property taxable As Boolean
                Public Property cost As Integer
                Public Property unitOfMeasure As String
                Public Property isInventory As Boolean
                Public Property modifiedOn As DateTime
                Public Property source As String
                Public Property externalId As String
            End Class

            Public Class SkuWarrantyResponse
                Public Property duration As Integer
                Public Property description As String
            End Class

            Public Class EquipmentUpdateRequest
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property active As Boolean
                Public Property manufacturer As String
                Public Property model As String
                Public Property manufacturerWarranty As SkuWarrantyRequest
                Public Property serviceProviderWarranty As SkuWarrantyRequest
                Public Property assets() As SkuAssetRequest
                Public Property recommendations() As SkuLinkRequest
                Public Property upgrades() As Integer
                Public Property equipmentMaterials() As SkuLinkRequest
                Public Property categories() As Integer
                Public Property primaryVendor As SkuVendorRequest
                Public Property otherVendors() As SkuVendorRequest
                Public Property account As String
                Public Property costOfSaleAccount As String
                Public Property assetAccount As String
                Public Property crossSaleGroup As String
                Public Property paysCommission As Boolean
                Public Property commissionBonus As Integer
                Public Property hours As Integer
                Public Property taxable As Boolean
                Public Property cost As Integer
                Public Property unitOfMeasure As String
                Public Property isInventory As Boolean
            End Class

            Public Class Manufacturerwarranty
                Public Property duration As Integer
                Public Property description As String
            End Class

            Public Class Serviceproviderwarranty
                Public Property duration As Integer
                Public Property description As String
            End Class

            Public Class SkuVendorRequest
                Public Property vendorId As Integer
                Public Property memo As String
                Public Property vendorPart As String
                Public Property cost As Integer
                Public Property active As Boolean
                Public Property primarySubAccount As SkuVendorSubAccountRequest
                Public Property otherSubAccounts() As SkuVendorSubAccountRequest
            End Class

            Public Class SkuVendorSubAccountRequest
                Public Property cost As Integer
                Public Property accountName As String
            End Class




            Public Class SkuLinkRequest
                Public Property skuId As Integer
                Public Property type As String
            End Class

            Public Class MaterialCreateRequest
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property cost As Integer
                Public Property active As Boolean
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property hours As Integer
                Public Property commissionBonus As Integer
                Public Property paysCommission As Boolean
                Public Property deductAsJobCost As Boolean
                Public Property unitOfMeasure As String
                Public Property isInventory As Boolean
                Public Property account As String
                Public Property costOfSaleAccount As String
                Public Property assetAccount As String
                Public Property taxable As Boolean
                Public Property primaryVendor As SkuVendorRequest
                Public Property otherVendors() As SkuVendorRequest
                Public Property assets() As SkuAssetResponse
                Public Property categories() As Integer
            End Class

            Public Class MaterialResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As MaterialResponse
                Public Property totalCount As Integer
            End Class

            Public Class MaterialResponse
                Public Property id As Integer
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property cost As Integer
                Public Property active As Boolean
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property hours As Integer
                Public Property commissionBonus As Integer
                Public Property paysCommission As Boolean
                Public Property deductAsJobCost As Boolean
                Public Property unitOfMeasure As String
                Public Property isInventory As Boolean
                Public Property account As String
                Public Property costOfSaleAccount As String
                Public Property assetAccount As String
                Public Property taxable As Boolean
                Public Property primaryVendor As SkuVendorResponse
                Public Property otherVendors() As SkuVendorResponse
                Public Property categories() As Integer
                Public Property assets() As SkuAssetResponse
                Public Property modifiedOn As DateTime
                Public Property source As String
                Public Property externalId As String
            End Class

            Public Class MaterialUpdateRequest
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property cost As Integer
                Public Property active As Boolean
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property hours As Integer
                Public Property commissionBonus As Integer
                Public Property paysCommission As Boolean
                Public Property deductAsJobCost As Boolean
                Public Property unitOfMeasure As String
                Public Property isInventory As Boolean
                Public Property account As String
                Public Property costOfSaleAccount As String
                Public Property assetAccount As String
                Public Property taxable As Boolean
                Public Property primaryVendor As SkuVendorRequest
                Public Property otherVendors() As SkuVendorRequest
                Public Property assets() As SkuAssetResponse
                Public Property categories() As Integer
            End Class


            Public Class ServiceCreateRequest
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property warranty As SkuWarrantyRequest
                Public Property categories() As Integer
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property taxable As Boolean
                Public Property account As String
                Public Property hours As Integer
                Public Property isLabor As Boolean
                Public Property recommendations() As Integer
                Public Property upgrades() As Integer
                Public Property assets() As SkuAssetResponse
                Public Property serviceMaterials() As SkuLinkResponse
                Public Property serviceEquipment() As SkuLinkResponse
                Public Property active As Boolean
                Public Property crossSaleGroup As String
                Public Property paysCommission As Boolean
                Public Property commissionBonus As Integer
            End Class

            Public Class ServiceResponse
                Public Property id As Integer
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property warranty As SkuWarrantyResponse
                Public Property categories() As SkuCategoryResponse
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property taxable As Boolean
                Public Property account As String
                Public Property hours As Integer
                Public Property isLabor As Boolean
                Public Property recommendations() As Integer
                Public Property upgrades() As Integer
                Public Property assets() As SkuAssetResponse
                Public Property serviceMaterials() As SkuLinkResponse
                Public Property serviceEquipment() As SkuLinkResponse
                Public Property active As Boolean
                Public Property crossSaleGroup As String
                Public Property paysCommission As Boolean
                Public Property modifiedOn As DateTime
                Public Property source As String
                Public Property externalId As String
            End Class



            Public Class SkuCategoryResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
            End Class

            Public Class ServiceResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As ServiceResponse
                Public Property totalCount As Integer
            End Class

            Public Class ServiceUpdateRequest
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property warranty As SkuWarrantyRequest
                Public Property categories() As Integer
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property taxable As Boolean
                Public Property account As String
                Public Property hours As Integer
                Public Property isLabor As Boolean
                Public Property recommendations() As Integer
                Public Property upgrades() As Integer
                Public Property assets() As SkuAssetResponse
                Public Property serviceMaterials() As SkuLinkResponse
                Public Property serviceEquipment() As SkuLinkResponse
                Public Property active As Boolean
                Public Property crossSaleGroup As String
                Public Property paysCommission As Boolean
                Public Property commissionBonus As Integer
            End Class














        End Class
        Public Class SalesTech

            Public Class CreateEstimateRequest
                Public Property name As String
                Public Property summary As String
                Public Property tax As Integer
                Public Property items() As EstimateItemCreateUpdateRequest
                Public Property externalLinks() As ExternalLinkInModel
                Public Property jobId As Integer
            End Class

            Public Class EstimateItemCreateUpdateRequest
                Public Property id As Integer
                Public Property skuId As Integer
                Public Property skuName As String
                Public Property parentItemId As Integer
                Public Property description As String
                Public Property isAddOn As Boolean
                Public Property quantity As Integer
                Public Property unitPrice As Integer
                Public Property skipUpdatingMembershipPrices As Boolean
                Public Property itemGroupName As String
                Public Property itemGroupRootId As Integer
            End Class

            Public Class ExternalLinkInModel
                Public Property name As String
                Public Property url As String
            End Class

            Public Class EstimateResponse
                Public Property id As Integer
                Public Property jobId As Integer
                Public Property projectId As Integer
                Public Property name As String
                Public Property jobNumber As String
                Public Property status As EstimateStatusModel
                Public Property summary As String
                Public Property modifiedOn As DateTime
                Public Property soldOn As DateTime
                Public Property soldBy As Integer
                Public Property items() As EstimateItemResponse
                Public Property externalLinks() As ExternalLinkResponse
            End Class

            Public Class EstimateStatusModel
                Public Property value As Integer
                Public Property name As String
            End Class

            Public Class EstimateItemResponse
                Public Property id As Integer
                Public Property sku As SkuModel
                Public Property skuAccount As String
                Public Property description As String
                Public Property qty As Integer
                Public Property unitRate As Integer
                Public Property total As Integer
                Public Property itemGroupName As String
                Public Property itemGroupRootId As Integer
                Public Property modifiedOn As DateTime
            End Class

            Public Class SkuModel
                Public Property id As Integer
                Public Property name As String
                Public Property displayName As String
                Public Property type As String
                Public Property soldHours As Integer
                Public Property generalLedgerAccountId As Integer
                Public Property generalLedgerAccountName As String
                Public Property modifiedOn As DateTime
            End Class

            Public Class ExternalLinkResponse
                Public Property name As String
                Public Property url As String
            End Class

            Public Class EstimateItemResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As EstimateItemResponse
                Public Property totalCount As Integer
            End Class

            Public Class EstimateResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As EstimateResponse
                Public Property totalCount As Integer
            End Class



            Public Class SellRequest
                Public Property soldBy As Integer
            End Class

            Public Class UpdateEstimateRequest
                Public Property name As String
                Public Property summary As String
                Public Property tax As Integer
                Public Property items() As EstimateItemCreateUpdateRequest
                Public Property externalLinks() As ExternalLinkInModel
            End Class






        End Class
        Public Class Settings

            Public Class BusinessUnitResponse
                Public Property id As Integer
                Public Property active As Boolean
                Public Property name As String
                Public Property officialName As String
                Public Property email As String
                Public Property currency As String
                Public Property phoneNumber As String
                Public Property invoiceHeader As String
                Public Property invoiceMessage As String
                Public Property defaultTaxRate As Integer
                Public Property authorizationParagraph As String
                Public Property acknowledgementParagraph As String
                Public Property address As BusinessUnitAddressResponse
                Public Property materialSku As String
                Public Property quickbooksClass As String
                Public Property accountCode As String
                Public Property franchiseId As String
                Public Property conceptCode As String
                Public Property corporateContractNumber As String
                Public Property tenant As BusinessUnitTenantResponse
                Public Property modifiedOn As DateTime
            End Class

            Public Class BusinessUnitAddressResponse
                Public Property street As String
                Public Property unit As String
                Public Property city As String
                Public Property state As String
                Public Property zip As String
                Public Property country As String
            End Class

            Public Class BusinessUnitTenantResponse
                Public Property id As Integer
                Public Property name As String
                Public Property quickbooksClass As String
                Public Property accountCode As String
                Public Property franchiseId As String
                Public Property conceptCode As String
                Public Property modifiedOn As DateTime
            End Class

            Public Class BusinessUnitResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As BusinessUnitResponse
                Public Property totalCount As Integer
            End Class

            Public Class EmployeeResponse
                Public Property id As Integer
                Public Property name As String
                Public Property role As EmployeeUserRole
                Public Property businessUnitId As Integer
                Public Property modifiedOn As DateTime
                Public Property email As String
                Public Property phoneNumber As String
                Public Property loginName As String
                Public Property customFields() As EmployeeCustomFieldResponse
                Public Property active As Boolean
            End Class

            Public Class EmployeeUserRole
                Public Property role As String
            End Class

            Public Class EmployeeCustomFieldResponse
                Public Property typeId As Integer
                Public Property name As String
                Public Property value As String
            End Class
            Public Class EmployeeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As EmployeeResponse
                Public Property totalCount As Integer
            End Class

            Public Class TechnicianResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data() As TechnicianResponse
                Public Property totalCount As Integer
            End Class

            Public Class TechnicianResponse
                Public Property id As Integer
                Public Property name As String
                Public Property businessUnitId As Integer
                Public Property modifiedOn As DateTime
                Public Property email As String
                Public Property phoneNumber As String
                Public Property loginName As String
                Public Property home As TechnicianAddressResponse
                Public Property dailyGoal As Integer
                Public Property isManagedTech As Boolean
                Public Property customFields() As TechnicianCustomFieldResponse
                Public Property active As Boolean
            End Class

            Public Class TechnicianAddressResponse
                Public Property street As String
                Public Property unit As String
                Public Property country As String
                Public Property city As String
                Public Property state As String
                Public Property zip As String
                Public Property streetAddress As String
                Public Property latitude As Integer
                Public Property longitude As Integer
            End Class

            Public Class TechnicianCustomFieldResponse
                Public Property typeId As Integer
                Public Property name As String
                Public Property value As String
            End Class

        End Class
        Public Class TaskManagement

            Public Class ClientSideDataResponse
                Public Property employees() As ClientSideEmployeeResponse
                Public Property businessUnits() As ClientSideBusinessUnitResponse
                Public Property taskPriorities() As ClientSideTaskPriorityResponse
                Public Property taskResolutionTypes() As ClientSideTaskResolutionTypeResponse
                Public Property taskStatuses() As ClientSideTaskStatusResponse
                Public Property taskTypes() As ClientSideTaskTypeResponse
                Public Property taskSources() As ClientSideTaskSourceResponse
                Public Property taskResolutions() As ClientSideTaskResolutionResponse
            End Class

            Public Class ClientSideEmployeeResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
            End Class

            Public Class ClientSideBusinessUnitResponse
                Public Property name As String
                Public Property value As Integer
            End Class

            Public Class ClientSideTaskPriorityResponse
                Public Property name As String
            End Class

            Public Class ClientSideTaskResolutionTypeResponse
                Public Property name As String
            End Class

            Public Class ClientSideTaskStatusResponse
                Public Property name As String
            End Class

            Public Class ClientSideTaskTypeResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
                Public Property excludedTaskResolutionIds() As Integer
            End Class

            Public Class ClientSideTaskSourceResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
            End Class

            Public Class ClientSideTaskResolutionResponse
                Public Property id As Integer
                Public Property name As String
                Public Property type As String
                Public Property active As Boolean
                Public Property excludedTaskTypeIds() As Integer
            End Class

            Public Class TaskCreateRequest
                Public Property reportedById As Integer
                Public Property assignedToId As Integer
                Public Property isClosed As Boolean
                Public Property name As String
                Public Property businessUnitId As Integer
                Public Property employeeTaskTypeId As Integer
                Public Property employeeTaskSourceId As Integer
                Public Property employeeTaskResolutionId As Integer
                Public Property reportedDate As String
                Public Property completeBy As String
                Public Property involvedEmployeeIdList() As Integer
                Public Property customerId As Integer
                Public Property jobId As Integer
                Public Property description As String
                Public Property priority As String
            End Class

            Public Class SubtaskCreateRequest
                Public Property isClosed As Boolean
                Public Property name As String
                Public Property assignedToId As Integer
                Public Property dueDateTime As String
            End Class

        End Class
        Public Class Telecom

            Public Class CallInModel
                Public Property callId As Integer
                Public Property createdOn As DateTime
                Public Property duration As String
                Public Property direction As CallDirection
                Public Property status As CallStatus
                Public Property callType As CallType
                Public Property excuseMemo As String
                Public Property campaignId As Integer
                Public Property jobId As Integer
                Public Property agentId As Integer
                Public Property recordingUrl As String
                Public Property recordingId As String
                Public Property reason As ReasonInModel
                Public Property customer As CustomerInModel
                Public Property location As LocationInModel
                Public Property sid As String
                Public Property from As DateTime
                <JsonProperty("to")>
                Public Property _to As DateTime
                Public Property callService As String
            End Class

            Public Class CallDirection
                Public Property direction As String
            End Class

            Public Class CallStatus
                Public Property status As String
            End Class

            Public Class CallType
                Public Property CallType As String
            End Class

            Public Class ReasonInModel
                Public Property name As String
                Public Property lead As Boolean
            End Class

            Public Class CustomerInModel
                Public Property id As Integer
                Public Property name As String
                Public Property address As AddressInput
                Public Property contacts() As ContactInputModel
            End Class

            Public Class AddressInput
                Public Property street As String
                Public Property unit As String
                Public Property country As String
                Public Property city As String
                Public Property state As String
                Public Property zip As String
                Public Property latitude As Integer
                Public Property longitude As Integer
            End Class

            Public Class ContactInputModel
                Public Property id As Integer
                Public Property type As String
                Public Property value As String
                Public Property memo As String
            End Class

            Public Class LocationInModel
                Public Property id As Integer
                Public Property name As String
                Public Property address As AddressInput
                Public Property contacts() As ContactInputModel
            End Class

            Public Class DetailedCallModel
                Public Property id As Integer
                Public Property receivedOn As DateTime
                Public Property duration As String
                Public Property from As DateTime
                <JsonProperty("to")>
                Public Property _to As DateTime
                Public Property direction As String
                Public Property callType As CallType
                Public Property reason As CallReasonModel
                Public Property recordingUrl As String
                Public Property voiceMailUrl As String
                Public Property createdBy As NamedModel
                Public Property customer As CustomerModel
                Public Property campaign As CampaignModel
                Public Property modifiedOn As DateTime
                Public Property agent As CallAgentModel
            End Class



            Public Class CallReasonModel
                Public Property id As Integer
                Public Property name As String
                Public Property lead As Boolean
                Public Property active As Boolean
            End Class

            Public Class NamedModel
                Public Property id As Integer
                Public Property name As String
            End Class

            Public Class CustomerModel
                Public Property id As Integer
                Public Property active As Boolean
                Public Property name As String
                Public Property email As String
                Public Property balance As Integer
                Public Property doNotMail As Boolean
                Public Property address As AddressOutput
                Public Property importId As String
                Public Property doNotService As Boolean
                Public Property type As String
                Public Property contacts() As ContactOutputModel
                Public Property mergedToId As Integer
                Public Property modifiedOn As DateTime
                Public Property memberships() As MembershipModel
                Public Property hasActiveMembership As Boolean
                Public Property customFields() As CustomFieldApiModel
                Public Property createdOn As DateTime
                Public Property createdBy As Integer
                Public Property phoneSettings() As CustomerPhoneModel
            End Class

            Public Class AddressOutput
                Public Property street As String
                Public Property unit As String
                Public Property country As String
                Public Property city As String
                Public Property state As String
                Public Property zip As String
                Public Property streetAddress As String
                Public Property latitude As Integer
                Public Property longitude As Integer
            End Class

            Public Class ContactOutputModel
                Public Property id As Integer
                Public Property type As String
                Public Property value As String
                Public Property memo As String
                Public Property active As Boolean
                Public Property modifiedOn As DateTime
            End Class

            Public Class MembershipModel
                Public Property id As Integer
                Public Property active As Boolean
                Public Property type As MembershipTypeModel
                Public Property status As String
                Public Property from As DateTime
                <JsonProperty("to")>
                Public Property _to As DateTime
                Public Property locationId As Integer
            End Class

            Public Class MembershipTypeModel
                Public Property id As Integer
                Public Property active As Boolean
                Public Property name As String
            End Class

            Public Class CustomFieldApiModel
                Public Property typeId As Integer
                Public Property name As String
                Public Property value As String
            End Class

            Public Class CustomerPhoneModel
                Public Property phoneNumber As String
                Public Property doNotText As Boolean
            End Class

            Public Class CampaignModel
                Public Property id As Integer
                Public Property name As String
                Public Property modifiedOn As DateTime
                Public Property active As Boolean
                Public Property category As CampaignCategoryModel
            End Class

            Public Class CampaignCategoryModel
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
            End Class

            Public Class CallAgentModel
                Public Property id As Integer
                Public Property name As String
                Public Property externalId As Integer
            End Class

            Public Class BundleCallModel_P
                Public Property data() As BundleCallModel
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property totalCount As Integer
                Public Property hasMore As Boolean
            End Class

            Public Class BundleCallModel
                Public Property id As Integer
                Public Property jobNumber As String
                Public Property projectId As Integer
                Public Property businessUnit As BusinessUnitModel
                Public Property type As JobTypeModel
                Public Property leadCall As CallModel
            End Class

            Public Class BusinessUnitModel
                Public Property id As Integer
                Public Property active As Boolean
                Public Property name As String
                Public Property officialName As String
                Public Property email As String
                Public Property currency As String
                Public Property phoneNumber As String
                Public Property invoiceHeader As String
                Public Property invoiceMessage As String
                Public Property defaultTaxRate As Integer
                Public Property authorizationParagraph As String
                Public Property acknowledgementParagraph As String
                Public Property address As BusinessUnitAddressModel
                Public Property materialSku As String
                Public Property quickbooksClass As String
                Public Property accountCode As String
                Public Property franchiseId As String
                Public Property conceptCode As String
                Public Property corporateContractNumber As String
                Public Property tenant As BusinessUnitTenantModel
                Public Property modifiedOn As DateTime
            End Class

            Public Class BusinessUnitAddressModel
                Public Property street As String
                Public Property unit As String
                Public Property city As String
                Public Property state As String
                Public Property zip As String
                Public Property country As String
            End Class

            Public Class BusinessUnitTenantModel
                Public Property id As Integer
                Public Property name As String
                Public Property quickbooksClass As String
                Public Property accountCode As String
                Public Property franchiseId As String
                Public Property conceptCode As String
                Public Property modifiedOn As DateTime
            End Class

            Public Class JobTypeModel
                Public Property id As Integer
                Public Property name As String
                Public Property modifiedOn As DateTime
            End Class

            Public Class CallModel
                Public Property id As Integer
                Public Property receivedOn As DateTime
                Public Property duration As String
                Public Property from As DateTime
                <JsonProperty("to")>
                Public Property _to As DateTime
                Public Property direction As String
                Public Property callType As CallType
                Public Property reason As CallReasonModel
                Public Property recordingUrl As String
                Public Property voiceMailUrl As String
                Public Property createdBy As NamedModel
                Public Property customer As CustomerModel
                Public Property campaign As CampaignModel
                Public Property modifiedOn As DateTime
                Public Property agent As CallAgentModel
            End Class

            Public Class DetailedBundleCallModel
                Public Property id As Integer
                Public Property jobNumber As String
                Public Property projectId As Integer
                Public Property businessUnit As BusinessUnitModel
                Public Property type As JobTypeModel
                Public Property leadCall As DetailedCallModel
            End Class

            Public Class CallInUpdateModel
                Public Property callId As Integer
                Public Property createdOn As DateTime
                Public Property duration As String
                Public Property direction As CallDirection
                Public Property status As CallStatus
                Public Property callType As CallType
                Public Property excuseMemo As String
                Public Property campaignId As Integer
                Public Property jobId As Integer
                Public Property agentId As Integer
                Public Property recordingUrl As String
                Public Property recordingId As String
                Public Property reason As ReasonInModel
                Public Property customer As CustomerInModel
                Public Property location As LocationInModel
            End Class
        End Class
    End Class
    Public Class Functions
        Inherits Internal
        Public Class Accounting
            Public Class Invoices
                Inherits Types.Accounting
                ''' <summary>
                ''' Creates a new adjustment invoice to an already existing invoice.
                ''' </summary>
                ''' <param name="payload">An instance of 'AdjustmentInvoiceCreateRequest'</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
                Public Shared Function createAdjustmentInvoices(ByVal payload As AdjustmentInvoiceCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)
                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And TimeSpan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - TimeSpan.TotalMilliseconds) + 100)
                        End If
                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxEnvironment
                        Else
                            domain = "https://" & productionEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/invoices"



                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "POST"
                        req.Timeout = 999999
                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)

                        Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                        req.ContentLength = bytearray.Length
                        req.Timeout = 999999
                        Dim datastream As Stream = req.GetRequestStream
                        datastream.Write(bytearray, 0, bytearray.Length)
                        datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Deletes a specific invoice item from a specific invoice.
                ''' </summary>
                ''' <param name="invoiceId">Invoice ID#</param>
                ''' <param name="invoiceItemId">Invoice Line Item ID#</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
                Public Shared Function deleteInvoiceItem(ByVal invoiceId As Integer, ByVal invoiceItemId As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)
                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If
                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxEnvironment
                        Else
                            domain = "https://" & productionEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/invoices/" & invoiceId & "/items/" & invoiceItemId



                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "DEL"
                        req.Timeout = 999999

                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)

                        'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                        'req.ContentLength = bytearray.Length
                        'req.Timeout = 999999
                        'Dim datastream As Stream = req.GetRequestStream
                        'datastream.Write(bytearray, 0, bytearray.Length)
                        'datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Gets a full list of invoices matching your specified filters. The current implementation of this end point does not support paging, so only page 1 can be retrieved.
                ''' </summary>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <param name="options">Your options, if you desire to filter the results (If applicable)</param>
                ''' <returns>Returns either a list of invoices (if successful) or a ServiceTitanError class (if the request fails).</returns>
                Public Shared Function getInvoiceList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If

                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxEnvironment
                        Else
                            domain = "https://" & productionEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/invoices"

                        Dim counter As Integer = 0
                        If options IsNot Nothing Then
                            If options.Count > 0 Then
                                counter = 1
                                For Each item In options
                                    If counter = 1 Then
                                        baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                                    Else
                                        baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                                    End If
                                    counter &= 1
                                Next
                            End If
                        End If


                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "GET"
                        req.Timeout = 999999
                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)
                        req.ContentType = "application/x-www-form-urlencoded"

                        'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                        'req.ContentLength = bytearray.Length
                        'req.Timeout = 999999
                        'Dim datastream As Stream = req.GetRequestStream
                        'datastream.Write(bytearray, 0, bytearray.Length)
                        'datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        Dim results As List(Of InvoiceResponse) = JsonConvert.DeserializeObject(Of List(Of InvoiceResponse))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                        Console.WriteLine("Output: " & output)
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                        Return results
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                'TODO: markAsExported: Bugged. Needs to be fixed properly.
                ''' <summary>
                ''' markAsExported: Endpoint is bugged (Does not allow specifying an invoiceid). Do not use!
                ''' </summary>
                ''' <param name="invoiceId"></param>
                ''' <param name="invoiceItemId"></param>
                ''' <param name="accesstoken"></param>
                ''' <param name="STAppKey"></param>
                ''' <param name="tenant"></param>
                ''' <returns></returns>
                Public Shared Function markAsExported(ByVal invoiceId As Integer, ByVal invoiceItemId As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)
                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If
                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxAuthEnvironment
                        Else
                            domain = "https://" & productionAuthEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/invoices/" & invoiceId & "/items/" & invoiceItemId



                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "DEL"
                        req.Timeout = 999999

                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)

                        'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                        'req.ContentLength = bytearray.Length
                        'req.Timeout = 999999
                        'Dim datastream As Stream = req.GetRequestStream
                        'datastream.Write(bytearray, 0, bytearray.Length)
                        'datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Updates an invoice's information.
                ''' </summary>
                ''' <param name="invoiceId">Invoice ID#</param>
                ''' <param name="invoice">An instance of InvoiceUpdateRequest</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
                Public Shared Function UpdateInvoice(ByVal invoiceId As Integer, ByVal invoice As InvoiceUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)
                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If
                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxAuthEnvironment
                        Else
                            domain = "https://" & productionAuthEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/invoices/" & invoiceId



                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "PATCH"
                        req.Timeout = 999999

                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)

                        Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(invoice))
                        req.ContentLength = bytearray.Length
                        req.Timeout = 999999
                        Dim datastream As Stream = req.GetRequestStream
                        datastream.Write(bytearray, 0, bytearray.Length)
                        datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Updates invoice items.
                ''' </summary>
                ''' <param name="invoiceId">Invoice ID#</param>
                ''' <param name="invoice">Invoice Items Payload (As InvoiceUpdateRequest)</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
                Public Shared Function UpdateInvoiceItems(ByVal invoiceId As Integer, ByVal invoice As InvoiceUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)
                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If
                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxAuthEnvironment
                        Else
                            domain = "https://" & productionAuthEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/invoices/" & invoiceId & "/items"



                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "PATCH"
                        req.Timeout = 999999

                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)

                        Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(invoice))
                        req.ContentLength = bytearray.Length
                        req.Timeout = 999999
                        Dim datastream As Stream = req.GetRequestStream
                        datastream.Write(bytearray, 0, bytearray.Length)
                        datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Updates custom fields in one or many invoices.
                ''' </summary>
                ''' <param name="customfields">Contains an array of invoices, with an array of custom fields.</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
                Public Shared Function UpdateInvoiceCustomFields(ByVal customfields As CustomFieldUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)
                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If
                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxAuthEnvironment
                        Else
                            domain = "https://" & productionAuthEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/invoices/custom-fields"



                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "PATCH"
                        req.Timeout = 999999

                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)

                        Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(customfields))
                        req.ContentLength = bytearray.Length
                        req.Timeout = 999999
                        Dim datastream As Stream = req.GetRequestStream
                        datastream.Write(bytearray, 0, bytearray.Length)
                        datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Adds a payment to an existing invoice.
                ''' </summary>
                ''' <param name="payment">PaymentCreateRequest payload definiting what payment to add, and where.</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>The new payment successfully created (As PaymentResponse), or a ServiceTitanError.</returns>
                Public Shared Function createPayment(ByVal payment As PaymentCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object
                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If
                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxAuthEnvironment
                        Else
                            domain = "https://" & productionAuthEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/payments"



                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "POST"
                        req.Timeout = 999999

                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)

                        Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payment))
                        req.ContentLength = bytearray.Length
                        req.Timeout = 999999
                        Dim datastream As Stream = req.GetRequestStream
                        datastream.Write(bytearray, 0, bytearray.Length)
                        datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd

                        streamread.Close()
                        buffer.Close()
                        response.Close()

                        Dim deserialized As PaymentResponse = JsonConvert.DeserializeObject(Of PaymentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                        Return deserialized
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Updates custom fields in one or many payments.
                ''' </summary>
                ''' <param name="customfields">Contains an array of invoices, with an array of custom fields.</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
                Public Shared Function UpdatePaymentCustomFields(ByVal customfields As CustomFieldUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)
                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If
                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxAuthEnvironment
                        Else
                            domain = "https://" & productionAuthEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/payments/custom-fields"



                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "PATCH"
                        req.Timeout = 999999

                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)

                        Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(customfields))
                        req.ContentLength = bytearray.Length
                        req.Timeout = 999999
                        Dim datastream As Stream = req.GetRequestStream
                        datastream.Write(bytearray, 0, bytearray.Length)
                        datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Updates one or many payments' statuses to one of the following: Pending, Posted or Exported.
                ''' </summary>
                ''' <param name="payment">The payment status change request(s)</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>If successful, nothing is returned. If the request ends in failure, an instance of ServiceTitanError is returned.</returns>
                Public Shared Function updatePaymentStatus(ByVal payment As PaymentStatusBatchRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)
                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If
                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxAuthEnvironment
                        Else
                            domain = "https://" & productionAuthEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/payments/status"



                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "POST"
                        req.Timeout = 999999

                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)

                        Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payment))
                        req.ContentLength = bytearray.Length
                        req.Timeout = 999999
                        Dim datastream As Stream = req.GetRequestStream
                        datastream.Write(bytearray, 0, bytearray.Length)
                        datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd

                        streamread.Close()
                        buffer.Close()
                        response.Close()

                        'Dim deserialized As PaymentResponse = JsonConvert.DeserializeObject(Of PaymentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                        'Return deserialized
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Updates a single payment.
                ''' </summary>
                ''' <param name="payment">Contains the payload to update the payment.</param>
                ''' <param name="paymentid">The payment unique ID</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>The new payment successfully updated (As PaymentResponse), or a ServiceTitanError.</returns>
                Public Shared Function updatePayment(ByVal payment As PaymentUpdateRequest, ByVal paymentid As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object
                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If
                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxAuthEnvironment
                        Else
                            domain = "https://" & productionAuthEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/payments/" & paymentid



                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "PATCH"
                        req.Timeout = 999999

                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)

                        Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payment))
                        req.ContentLength = bytearray.Length
                        req.Timeout = 999999
                        Dim datastream As Stream = req.GetRequestStream
                        datastream.Write(bytearray, 0, bytearray.Length)
                        datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd

                        streamread.Close()
                        buffer.Close()
                        response.Close()

                        Dim deserialized As PaymentResponse = JsonConvert.DeserializeObject(Of PaymentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                        Return deserialized
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                'TODO: Implement pagination once it is implemented API-side
                ''' <summary>
                ''' Gets a list of payments depending on filters set. This API currently has arguments for pagination, but the json output has no support for it. So pagination has to be guessed.
                ''' </summary>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <param name="options">A list of filters or preferences for this function</param>
                ''' <returns>Either returns a (unpaginated) list of payments, or a ServiceTitanError.</returns>
                Public Shared Function getPaymentsList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If

                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxEnvironment
                        Else
                            domain = "https://" & productionEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/payments"

                        Dim counter As Integer = 0
                        If options IsNot Nothing Then
                            If options.Count > 0 Then
                                counter = 1
                                For Each item In options
                                    If counter = 1 Then
                                        baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                                    Else
                                        baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                                    End If
                                    counter &= 1
                                Next
                            End If
                        End If


                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "GET"
                        req.Timeout = 999999
                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)
                        req.ContentType = "application/x-www-form-urlencoded"

                        'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                        'req.ContentLength = bytearray.Length
                        'req.Timeout = 999999
                        'Dim datastream As Stream = req.GetRequestStream
                        'datastream.Write(bytearray, 0, bytearray.Length)
                        'datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        Dim results As List(Of DetailedPaymentResponse) = JsonConvert.DeserializeObject(Of List(Of DetailedPaymentResponse))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                        Console.WriteLine("Output: " & output)
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                        Return results
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Gets the current default payment terms for the specified customer.
                ''' </summary>
                ''' <param name="customerid">The requested customer's ID</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>Returns either an instance of PaymentTermModel, or a ServiceTitanError.</returns>
                Public Shared Function getCustomerDefaultPaymentTerm(ByVal customerid As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If

                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxEnvironment
                        Else
                            domain = "https://" & productionEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/payment-terms/" & customerid

                        'Dim counter As Integer = 0
                        'If options IsNot Nothing Then
                        '    If options.Count > 0 Then
                        '        counter = 1
                        '        For Each item In options
                        '            If counter = 1 Then
                        '                baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                        '            Else
                        '                baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                        '            End If
                        '            counter &= 1
                        '        Next
                        '    End If
                        'End If


                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "GET"
                        req.Timeout = 999999
                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)
                        req.ContentType = "application/x-www-form-urlencoded"

                        'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                        'req.ContentLength = bytearray.Length
                        'req.Timeout = 999999
                        'Dim datastream As Stream = req.GetRequestStream
                        'datastream.Write(bytearray, 0, bytearray.Length)
                        'datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        Dim results As List(Of PaymentTermModel) = JsonConvert.DeserializeObject(Of List(Of PaymentTermModel))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                        Console.WriteLine("Output: " & output)
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                        Return results
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Gets a paginated list of payment types. (Note: Does not fetch any subsequent pages, use additional options to go through them)
                ''' </summary>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <param name="options">A list of filters or preferences for this function</param>
                ''' <returns>Returns either a paginated list of payment types (PaymentTypeResponse_P) or a ServiceTitanError.</returns>
                Public Shared Function getPaymentTypesList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If

                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxEnvironment
                        Else
                            domain = "https://" & productionEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/payment-types"

                        Dim counter As Integer = 0
                        If options IsNot Nothing Then
                            If options.Count > 0 Then
                                counter = 1
                                For Each item In options
                                    If counter = 1 Then
                                        baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                                    Else
                                        baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                                    End If
                                    counter &= 1
                                Next
                            End If
                        End If


                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "GET"
                        req.Timeout = 999999
                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)
                        req.ContentType = "application/x-www-form-urlencoded"

                        'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                        'req.ContentLength = bytearray.Length
                        'req.Timeout = 999999
                        'Dim datastream As Stream = req.GetRequestStream
                        'datastream.Write(bytearray, 0, bytearray.Length)
                        'datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        Dim results As List(Of PaymentTypeResponse_P) = JsonConvert.DeserializeObject(Of List(Of PaymentTypeResponse_P))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                        Console.WriteLine("Output: " & output)
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                        Return results
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Gets a single payment type, by it's ID.
                ''' </summary>
                ''' <param name="PaymentTypeID">The payment type ID.</param>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <returns>Returns either a payment type (PaymentTypeResponse) or a ServiceTitanError.</returns>
                Public Shared Function getPaymentType(ByVal PaymentTypeID As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If

                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxEnvironment
                        Else
                            domain = "https://" & productionEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/payment-types/" & PaymentTypeID

                        'Dim counter As Integer = 0
                        'If options IsNot Nothing Then
                        '    If options.Count > 0 Then
                        '        counter = 1
                        '        For Each item In options
                        '            If counter = 1 Then
                        '                baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                        '            Else
                        '                baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                        '            End If
                        '            counter &= 1
                        '        Next
                        '    End If
                        'End If


                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "GET"
                        req.Timeout = 999999
                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)
                        req.ContentType = "application/x-www-form-urlencoded"

                        'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                        'req.ContentLength = bytearray.Length
                        'req.Timeout = 999999
                        'Dim datastream As Stream = req.GetRequestStream
                        'datastream.Write(bytearray, 0, bytearray.Length)
                        'datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        Dim results As List(Of PaymentTypeResponse) = JsonConvert.DeserializeObject(Of List(Of PaymentTypeResponse))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                        Console.WriteLine("Output: " & output)
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                        Return results
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
                ''' <summary>
                ''' Returns a paginated list of tax zones. Does not get other pages, use pagination to cycle through them.
                ''' </summary>
                ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
                ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
                ''' <param name="tenant">Your Tenant ID</param>
                ''' <param name="options">A list of filters or preferences for this function</param>
                ''' <returns>Returns either a paginated list of tax zones (TaxZoneResponse_P) or a ServiceTitanError.</returns>
                Public Shared Function getTaxZonesList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

                    Try
                        Dim timespan As TimeSpan = Now - lastQuery
                        If lastQuery <> DateTime.MinValue And timespan.TotalMilliseconds < minMsSinceLastQuery Then
                            'Try to avoid getting hit by the rate limiter by sleeping it off
                            Threading.Thread.Sleep((minMsSinceLastQuery - timespan.TotalMilliseconds) + 100)
                        End If

                        Dim domain As String
                        If useSandbox = True Then
                            domain = "https://" & sandboxEnvironment
                        Else
                            domain = "https://" & productionEnvironment

                        End If
                        Dim baseurl As String = domain & "/accounting/v2/tenant/" & tenant & "/tax-zones"

                        Dim counter As Integer = 0
                        If options IsNot Nothing Then
                            If options.Count > 0 Then
                                counter = 1
                                For Each item In options
                                    If counter = 1 Then
                                        baseurl &= "?" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                                    Else
                                        baseurl &= "&" & System.Net.WebUtility.UrlEncode(item.key) & "=" & System.Net.WebUtility.UrlEncode(item.value)
                                    End If
                                    counter &= 1
                                Next
                            End If
                        End If


                        Console.WriteLine("Executing: " & baseurl)
                        Dim req As WebRequest = WebRequest.Create(baseurl)
                        req.Method = "GET"
                        req.Timeout = 999999
                        req.Headers.Add("ST-App-Key", STAppKey)
                        req.Headers.Add("Authorization", accesstoken.access_token)
                        req.ContentType = "application/x-www-form-urlencoded"

                        'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                        'req.ContentLength = bytearray.Length
                        'req.Timeout = 999999
                        'Dim datastream As Stream = req.GetRequestStream
                        'datastream.Write(bytearray, 0, bytearray.Length)
                        'datastream.Close()

                        Dim response As WebResponse = req.GetResponse
                        Dim buffer As Stream = response.GetResponseStream
                        Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                        Dim output As String = streamread.ReadToEnd
                        Dim results As List(Of TaxZoneResponse_P) = JsonConvert.DeserializeObject(Of List(Of TaxZoneResponse_P))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                        Console.WriteLine("Output: " & output)
                        streamread.Close()
                        buffer.Close()
                        response.Close()
                        Return results
                    Catch ex As WebException
                        Dim newerror As ApiErrorResponse = ErrorHandling.ProcessError(ex)
                        Return newerror
                    End Try
                End Function
            End Class

        End Class
    End Class
    Public Class Internal
        ''' <summary>
        ''' OptionsList: Used to enumerate HTTP arguments, mainly used when GETting data from the API as a way to filter the data returned.
        ''' </summary>
        Public Class OptionsList
            Public key As String
            Public value As String
        End Class
    End Class
    Public Class ApiErrorResponse
        Public type As String
        Public title As String
        Public status As Integer
        Public traceId As String
        Public errors As String
        Public data As String
    End Class
    Public Class ServiceTitanError
        Public ApiErrorResponse As New ApiErrorResponse
        Public WebError As New WebException
    End Class
    Public Class ErrorHandling
        Public Shared Function ProcessError(ByVal exception As WebException)

            Console.WriteLine("STAPIv2: -----An Error Has Occured-----")

            Console.WriteLine("HTTP Request Code: " & DirectCast(exception.Response, System.Net.HttpWebResponse).StatusCode & " (" & DirectCast(exception.Response, System.Net.HttpWebResponse).StatusDescription & ")")
            Dim str As Stream = exception.Response.GetResponseStream
            Dim output As StreamReader = New StreamReader(str)
            Dim outputstr As String = output.ReadToEnd
            Console.WriteLine("ServiceTitan returned: " & outputstr)
            Console.WriteLine("StackTrace: " & exception.StackTrace)
            Console.WriteLine("STAPIv2: -----End of Error-----")
            Dim newerror As New ServiceTitanError
            newerror.ApiErrorResponse = JsonConvert.DeserializeObject(Of ApiErrorResponse)(outputstr, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
            newerror.WebError = exception
            Return newerror
        End Function
    End Class
End Class
