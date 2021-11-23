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
                Public Property items As New InvoiceItemUpdateRequest
                Public Property payments As List(Of PaymentSettlementUpdateRequest)
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
                Public Property items As List(Of InvoiceItemResponse)
                Public Property customFields As List(Of CustomFieldResponse)
            End Class

            Public Class SalesTaxResponse
                Public Property id As Integer
                Public Property name As String
                Public Property taxRate As Decimal
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
            Public Class InvoiceResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of InvoiceResponse)
                Public Property totalCount As Integer
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
                Public Property soldHours As Decimal
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
                Public Property items As List(Of InvoiceItemUpdateRequest)
                Public Property payments As List(Of PaymentSettlementUpdateRequest)
            End Class

            Public Class CustomFieldUpdateRequest
                Public Property operations As List(Of CustomFieldOperationRequest)
            End Class

            Public Class CustomFieldOperationRequest
                Public Property objectId As Integer
                Public Property customFields As List(Of CustomFieldPairRequest)
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
                Public Property splits As List(Of PaymentSplitApiModel)
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

            Public Class PaymentResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of PaymentResponse)
                Public Property totalCount As Integer
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
                Public Property splits As List(Of PaymentSplitApiModel)
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
                Public Property appliedTo As List(Of PaymentAppliedResponse)
                Public Property customFields As List(Of CustomFieldModel)
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
                Public Property paymentIds As List(Of Integer)
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
                Public Property splits As List(Of PaymentSplitApiModel)
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
                Public Property data As List(Of PaymentTypeResponse)
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
                Public Property data As List(Of TaxZoneResponse)
                Public Property totalCount As Integer
            End Class

            Public Class TaxZoneResponse
                Public Property id As Integer
                Public Property name As String
                Public Property color As Integer
                Public Property isTaxRateSeparated As Boolean
                Public Property isMultipleTaxZone As Boolean
                Public Property rates As List(Of TaxRateResponse)
                Public Property createdOn As DateTime
                Public Property active As Boolean
            End Class

            Public Class TaxRateResponse
                Public Property id As Integer
                Public Property taxName As String
                Public Property taxBaseType As List(Of String)
                Public Property taxRate As Decimal
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
                Public Property data As List(Of LeadResponse)
                Public Property totalCount As Integer
            End Class

            Public Class TagsResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of TagsResponse)
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
                Public Property data As List(Of AppointmentAssignmentResponse)
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

            Public Class CapacityQueryFilter
                Public Property startsOnOrAfter As DateTime
                Public Property endsOnOrBefore As DateTime
                Public Property businessUnitIds As List(Of Integer)
                Public Property jobTypeId As Integer
                Public Property skillBasedAvailability As Boolean
            End Class


            Public Class CapacityResponse
                Public Property startsOnOrAfter As String
                Public Property endsOnOrBefore As String
                Public Property businessUnitIds As List(Of Integer)
                Public Property jobTypeId As Integer
                Public Property skillBasedAvailability As Boolean
            End Class

            Public Class TechnicianShiftResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of TechnicianShiftResponse)
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
                Public Property customFields As List(Of CustomFieldRequestModel)
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
                Public Property customFields As List(Of CustomFieldResponseModel)
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
                Public Property data As List(Of InstalledEquipmentResponse)
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
                Public Property customFields As List(Of CustomFieldRequestModel)
            End Class



        End Class
        Public Class Inventory
            Public Class CreatePurchaseOrderResponse
                Public id As Integer
            End Class

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
                Public Property items As List(Of CreatePurchaseOrderItemRequest)
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
                Public Property data As List(Of PurchaseOrderResponse)
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
                Public Property items As List(Of PurchaseOrderItemResponse)
                Public Property customFields As List(Of CustomFieldApiModel)
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
                Public Property serialNumbers As List(Of SerialNumberResponse)
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
                Public Property items As List(Of UpdatePurchaseOrderItemRequest)
                Public Property removedItems As List(Of RemovePurchaseOrderItemRequest)
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
                Public Property data As List(Of VendorResponse)
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
                Public Property data As List(Of CallReasonResponse)
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
                Public Property technicianIds As List(Of Integer)
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
                Public Property data As List(Of AppointmentResponse)
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
                Public Property data As List(Of JobCancelReasonResponse)
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
                Public Property data As List(Of JobHoldReasonResponse)
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
                Public Property appointments As List(Of AppointmentInformation)
                Public Property summary As String
                Public Property customFields As List(Of CustomFieldApiModel)
                Public Property tagTypeIds As List(Of Integer)
                Public Property externalData As ExternalDataUpdateRequest
            End Class

            Public Class ExternalDataUpdateRequest
                Public Property applicationGuid As String
                Public Property externalData As List(Of ExternalDataModel)
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
                Public Property technicianIds As List(Of Integer)
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
                Public Property customFields As List(Of CustomFieldApiModel)
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
                Public Property tagTypeIds As List(Of Integer)
                Public Property leadCallId As Integer
                Public Property bookingId As Integer
                Public Property soldById As Integer
                Public Property externalData As List(Of ExternalDataUpdateRequest)
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
                Public Property data As List(Of CancelReasonResponse)
                Public Property totalCount As Integer
            End Class

            Public Class CancelReasonResponse
                Public Property jobId As Integer
                Public Property reasonId As Integer
                Public Property name As String
                Public Property text As String
            End Class

            Public Class JobHistoryResponse
                Public Property history As List(Of JobHistoryItemModel)
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
                Public Property data As List(Of JobResponse)
                Public Property totalCount As Integer
            End Class

            Public Class NoteResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of NoteResponse)
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
                Public Property customFields As List(Of CustomFieldApiModel)
                Public Property tagIds As List(Of Integer)
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
                Public Property externalData As List(Of ExternalDataUpdateRequest)
            End Class

            Public Class JobTypeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of JobTypeResponse)
                Public Property totalCount As Integer
            End Class

            Public Class ProjectResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of ProjectResponse)
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
                Public Property customFields As List(Of CustomFieldApiModel)
            End Class















        End Class
        Public Class Marketing

            Public Class CampaignCategoryCreateUpdateModel
                Public Property name As String
                Public Property active As Boolean
            End Class

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
                Public Property data As List(Of CampaignModel)
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
                Public Property data As List(Of CustomerMembershipResponse)
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
                Public Property items As List(Of InvoiceTemplateItemCreateRequest)
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
                Public Property data As List(Of InvoiceTemplateResponse)
                Public Property totalCount As Integer
            End Class

            Public Class InvoiceTemplateResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
                Public Property total As Integer
                Public Property isSettingsTemplate As Boolean
                Public Property importId As String
                Public Property items As List(Of InvoiceTemplateItemResponse)
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
                Public Property items As List(Of InvoiceTemplateItemUpdateRequest)
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
                Public Property data As List(Of LocationRecurringServiceEventResponse)
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
                Public Property data As List(Of LocationRecurringServiceResponse)
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
                Public Property recurrenceMonths As List(Of String)
                Public Property recurrenceDaysOfWeek As List(Of String)
                Public Property recurrenceWeek As WeekDay
                Public Property recurrenceDayOfNthWeek As DayOfWeek
                Public Property recurrenceDaysOfMonth As List(Of Integer)
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
                Public Property recurrenceMonths As List(Of String)
                Public Property recurrenceDaysOfWeek As List(Of String)
                Public Property recurrenceWeek As String
                Public Property recurrenceDayOfNthWeek As DayOfWeek
                Public Property jobStartTime As String
                Public Property estimatedPayrollCost As Integer
            End Class

            Public Class MembershipTypeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of MembershipTypeResponse)
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
                Public Property data As List(Of RecurringServiceTypeResponse)
                Public Property totalCount As Integer
            End Class

            Public Class RecurringServiceTypeResponse
                Public Property id As Integer
                Public Property active As Boolean
                Public Property recurrenceType As ServiceRecurrenceType
                Public Property recurrenceInterval As Integer
                Public Property recurrenceMonths As List(Of String)
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
            Public Class ModificationResponse
                Public id As Integer
            End Class

            Public Class PayrollActivityCodeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of PayrollActivityCodeResponse)
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
                Public Property data As List(Of GrossPayItemResponse)
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
                Public Property data As List(Of PayrollAdjustmentResponse)
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
                Public Property data As List(Of PayrollResponse)
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
                Public Property data As List(Of TimesheetCodeResponse)
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
                Public Property businessUnitIds As List(Of Integer)
                Public Property skuImages As List(Of String)
                Public Property skuVideos As List(Of String)
            End Class
            Public Class CategoryResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property totalCount As Integer
                Public Property data As List(Of CategoryResponse)
            End Class
            Public Class CategoryResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
                Public Property description As String
                Public Property image As String
                Public Property parentId As Integer
                Public Property position As Integer
                Public Property categoryType As CategoryType
                Public Property subcategories As CategoryResponse
                Public Property businessUnitIds As List(Of Integer)
                Public Property skuImages As List(Of String)
                Public Property skuVideos As List(Of String)
                Public Property source As String
                Public Property externalId As String
            End Class
            Public Class CategoryType
                Public Property type As String
            End Class
            Public Class CategoryUpdateRequest
                Public Property name As String
                Public Property active As Boolean
                Public Property description As String
                Public Property parentId As Integer
                Public Property position As Integer
                Public Property image As String
                Public Property categoryType As String
                Public Property businessUnitIds As List(Of Integer)
                Public Property skuImages As List(Of String)
                Public Property skuVideos As List(Of String)
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
                Public Property categories As List(Of Integer)
                Public Property hours As Integer
                Public Property assets As List(Of SkuAssetResponse)
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
                Public Property categories As List(Of Integer)
                Public Property hours As Integer
                Public Property assets As List(Of SkuAssetResponse)
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
                Public Property data As List(Of DiscountAndFeesResponse)
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
                Public Property categories As List(Of Integer)
                Public Property hours As Integer
                Public Property assets As List(Of SkuAssetRequest)
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
                Public Property assets As List(Of SkuAssetRequest)
                Public Property recommendations As List(Of EquipmentRecommendationResponse)
                Public Property upgrades As List(Of Integer)
                Public Property equipmentMaterials As List(Of SkuLinkResponse)
                Public Property categories As List(Of Integer)
                Public Property primaryVendor As SkuVendorResponse
                Public Property otherVendors As List(Of SkuVendorResponse)
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
                Public Property otherSubAccounts As List(Of SkuVendorSubAccountResponse)
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
            Public Class EquipmentResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property totalCount As Integer
                Public Property data As List(Of EquipmentResponse)
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
                Public Property categories As List(Of Integer)
                Public Property assets As List(Of SkuAssetResponse)
                Public Property recommendations As List(Of EquipmentRecommendationResponse)
                Public Property upgrades As List(Of Integer)
                Public Property equipmentMaterials As List(Of SkuLinkResponse)
                Public Property primaryVendor As SkuVendorResponse
                Public Property otherVendors As List(Of SkuVendorResponse)
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
                Public Property assets As List(Of SkuAssetRequest)
                Public Property recommendations As List(Of SkuLinkRequest)
                Public Property upgrades As List(Of Integer)
                Public Property equipmentMaterials As List(Of SkuLinkRequest)
                Public Property categories As List(Of Integer)
                Public Property primaryVendor As SkuVendorRequest
                Public Property otherVendors As List(Of SkuVendorRequest)
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
                Public Property otherSubAccounts As List(Of SkuVendorSubAccountRequest)
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
                Public Property otherVendors As List(Of SkuVendorRequest)
                Public Property assets As List(Of SkuAssetResponse)
                Public Property categories As List(Of Integer)
            End Class

            Public Class MaterialResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of MaterialResponse)
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
                Public Property otherVendors As List(Of SkuVendorResponse)
                Public Property categories As List(Of Integer)
                Public Property assets As List(Of SkuAssetResponse)
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
                Public Property otherVendors As List(Of SkuVendorRequest)
                Public Property assets As List(Of SkuAssetResponse)
                Public Property categories As List(Of Integer)
            End Class


            Public Class ServiceCreateRequest
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property warranty As SkuWarrantyRequest
                Public Property categories As List(Of Integer)
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property taxable As Boolean
                Public Property account As String
                Public Property hours As Integer
                Public Property isLabor As Boolean
                Public Property recommendations As List(Of Integer)
                Public Property upgrades As List(Of Integer)
                Public Property assets As List(Of SkuAssetResponse)
                Public Property serviceMaterials As List(Of SkuLinkResponse)
                Public Property serviceEquipment As List(Of SkuLinkResponse)
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
                Public Property categories As List(Of SkuCategoryResponse)
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property taxable As Boolean
                Public Property account As String
                Public Property hours As Integer
                Public Property isLabor As Boolean
                Public Property recommendations As List(Of Integer)
                Public Property upgrades As List(Of Integer)
                Public Property assets As List(Of SkuAssetResponse)
                Public Property serviceMaterials As List(Of SkuLinkResponse)
                Public Property serviceEquipment As List(Of SkuLinkResponse)
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
                Public Property data As List(Of ServiceResponse)
                Public Property totalCount As Integer
            End Class

            Public Class ServiceUpdateRequest
                Public Property code As String
                Public Property displayName As String
                Public Property description As String
                Public Property warranty As SkuWarrantyRequest
                Public Property categories As List(Of Integer)
                Public Property price As Integer
                Public Property memberPrice As Integer
                Public Property addOnPrice As Integer
                Public Property addOnMemberPrice As Integer
                Public Property taxable As Boolean
                Public Property account As String
                Public Property hours As Integer
                Public Property isLabor As Boolean
                Public Property recommendations As List(Of Integer)
                Public Property upgrades As List(Of Integer)
                Public Property assets As List(Of SkuAssetResponse)
                Public Property serviceMaterials As List(Of SkuLinkResponse)
                Public Property serviceEquipment As List(Of SkuLinkResponse)
                Public Property active As Boolean
                Public Property crossSaleGroup As String
                Public Property paysCommission As Boolean
                Public Property commissionBonus As Integer
            End Class














        End Class
        Public Class SalesAndEstimates

            Public Class CreateEstimateRequest
                Public Property name As String
                Public Property summary As String
                Public Property tax As Integer
                Public Property items As List(Of EstimateItemCreateUpdateRequest)
                Public Property externalLinks As List(Of ExternalLinkInModel)
                Public Property jobId As Integer
            End Class

            Public Class EstimateItemUpdateResponse
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
                Public Property estimateId As Integer
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
                Public Property items As List(Of EstimateItemResponse)
                Public Property externalLinks As List(Of ExternalLinkResponse)
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
                Public Property soldHours As Decimal
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
                Public Property data As List(Of EstimateItemResponse)
                Public Property totalCount As Integer
            End Class

            Public Class EstimateResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of EstimateResponse)
                Public Property totalCount As Integer
            End Class



            Public Class SellRequest
                Public Property soldBy As Integer
            End Class

            Public Class UpdateEstimateRequest
                Public Property name As String
                Public Property summary As String
                Public Property tax As Integer
                Public Property items As List(Of EstimateItemCreateUpdateRequest)
                Public Property externalLinks As List(Of ExternalLinkInModel)
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
                Public Property data As List(Of BusinessUnitResponse)
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
                Public Property customFields As List(Of EmployeeCustomFieldResponse)
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
                Public Property data As List(Of EmployeeResponse)
                Public Property totalCount As Integer
            End Class

            Public Class TagTypeResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property totalCount As Integer
                Public Property data As List(Of TagTypeResponse)
            End Class

            Public Class TagTypeResponse
                Public Property id As Integer
                Public Property name As String
                Public Property active As Boolean
            End Class

            Public Class TechnicianResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property data As List(Of TechnicianResponse)
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
                Public Property customFields As List(Of TechnicianCustomFieldResponse)
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
                Public Property employees As List(Of ClientSideEmployeeResponse)
                Public Property businessUnits As List(Of ClientSideBusinessUnitResponse)
                Public Property taskPriorities As List(Of ClientSideTaskPriorityResponse)
                Public Property taskResolutionTypes As List(Of ClientSideTaskResolutionTypeResponse)
                Public Property taskStatuses As List(Of ClientSideTaskStatusResponse)
                Public Property taskTypes As List(Of ClientSideTaskTypeResponse)
                Public Property taskSources As List(Of ClientSideTaskSourceResponse)
                Public Property taskResolutions As List(Of ClientSideTaskResolutionResponse)
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
                Public Property excludedTaskResolutionIds As List(Of Integer)
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
                Public Property excludedTaskTypeIds As List(Of Integer)
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
                Public Property involvedEmployeeIdList As List(Of Integer)
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

            Public Class SubTaskCreateResponse
                Public Property isClosed As Boolean
                Public Property name As String
                Public Property assignedToId As Integer
                Public Property dueDateTime As DateTime
                Public Property parentTaskId As Integer
                Public Property subtaskNumber As String
                Public Property isViewed As Boolean
                Public Property assignedDateTime As DateTime
                Public Property createdOn As DateTime
            End Class

            Public Class TaskCreateResponse
                Public Property reportedById As Integer
                Public Property assignedToId As Integer
                Public Property isClosed As Boolean
                Public Property name As String
                Public Property businessUnitId As Integer
                Public Property employeeTaskTypeId As Integer
                Public Property employeeTaskSourceId As Integer
                Public Property employeeTaskResolutionId As Integer
                Public Property reportedDate As DateTime
                Public Property completeBy As DateTime
                Public Property involvedEmployeeIdList As List(Of Integer)
                Public Property customerId As Integer
                Public Property jobId As Integer
                Public Property description As String
                Public Property priority As String
                Public Property id As Integer
                Public Property taskNumber As Integer
                Public Property customerName As String
                Public Property jobNumber As String
                Public Property refundIssued As Integer
                Public Property descriptionModifiedOn As DateTime
                Public Property descriptionModifiedBy As String
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
                Public Property contacts As List(Of ContactInputModel)
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
                Public Property contacts As List(Of ContactInputModel)
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



            Public Class CallReasonResponse_P
                Public Property page As Integer
                Public Property pageSize As Integer
                Public Property hasMore As Boolean
                Public Property totalCount As Integer
                Public Property data As List(Of CallReasonResponse)
            End Class

            Public Class CallReasonResponse
                Public Property id As Integer
                Public Property name As String
                Public Property isLead As Boolean
                Public Property active As Boolean
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
                Public Property contacts As List(Of ContactOutputModel)
                Public Property mergedToId As Integer
                Public Property modifiedOn As DateTime
                Public Property memberships As List(Of MembershipModel)
                Public Property hasActiveMembership As Boolean
                Public Property customFields As List(Of CustomFieldApiModel)
                Public Property createdOn As DateTime
                Public Property createdBy As Integer
                Public Property phoneSettings As List(Of CustomerPhoneModel)
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
                Public Property data As List(Of BundleCallModel)
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
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a full list of invoices matching your specified filters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">Your options, if you desire to filter the results (If applicable)</param>
            ''' <returns>Returns either a paginated list of invoices (InvoiceResponse_P) or a ServiceTitanError class.</returns>
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
                    Dim results As InvoiceResponse_P = JsonConvert.DeserializeObject(Of InvoiceResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim results As PaymentTypeResponse_P = JsonConvert.DeserializeObject(Of PaymentTypeResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim results As PaymentTypeResponse = JsonConvert.DeserializeObject(Of PaymentTypeResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
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
                    Dim results As TaxZoneResponse_P = JsonConvert.DeserializeObject(Of TaxZoneResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function

        End Class
        Public Class CRM
            Inherits Types.CRM
            ''' <summary>
            ''' Gets a specific lead by it's internal ID.
            ''' </summary>
            ''' <param name="leadid">The lead ID number</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a single lead (LeadResponse) or a ServiceTitanError.</returns>
            Public Shared Function getLead(ByVal leadid As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/crm/v2/tenant/" & tenant & "/leads"

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
                    Dim results As LeadResponse = JsonConvert.DeserializeObject(Of LeadResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Returns a paginated list of leads. Does not get other pages, use pagination to cycle through them.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of filters or preferences for this function</param>
            ''' <returns>Returns either a paginated list of Leads (LeadResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getLeadList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/crm/v2/tenant/" & tenant & "/leads"

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
                    Dim results As LeadResponse_P = JsonConvert.DeserializeObject(Of LeadResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Returns a paginated list of tags. Does not get other pages, use pagination to cycle through them.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of filters or preferences for this function</param>
            ''' <returns>Returns either a paginated list of tags (TagsResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getTagList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/crm/v2/tenant/" & tenant & "/tags"

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
                    Dim results As TagsResponse_P = JsonConvert.DeserializeObject(Of TagsResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class Dispatch
            Inherits Types.Dispatch
            ''' <summary>
            ''' Returns a paginated list of appointment assignments. Does not get other pages, use pagination to cycle through them.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of filters or preferences for this function</param>
            ''' <returns>Returns either a paginated list of appointment assignments (AppointmentAssignmentResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getAppointmentAssignmentList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/dispatch/v2/tenant/" & tenant & "/appointment-assignments"

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
                    Dim results As AppointmentAssignmentResponse_P = JsonConvert.DeserializeObject(Of AppointmentAssignmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets the availability for the requested timeframe using the information provided in the CapacityQueryFilter.
            ''' </summary>
            ''' <param name="payload">Your request parameters for the filter.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either the current availabilities (CapacityResponse) or a ServiceTitanError.</returns>
            Public Shared Function getCapacity(ByVal payload As CapacityQueryFilter, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/dispatch/v2/tenant/" & tenant & "/capacity"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As CapacityResponse = JsonConvert.DeserializeObject(Of CapacityResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Returns a paginated list of technician shifts. Does not get other pages, use pagination to cycle through them.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of filters or preferences for this function</param>
            ''' <returns>Returns either a paginated list of technician shifts (TechnicianShiftResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getTechnicianShifts(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/dispatch/v2/tenant/" & tenant & "/technician-shifts"

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
                    req.ContentType = "application/json"

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
                    Dim results As TechnicianShiftResponse_P = JsonConvert.DeserializeObject(Of TechnicianShiftResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single technician shift, by it's ID.
            ''' </summary>
            ''' <param name="id">The unique id of the shift record</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a technician shifts (TechnicianShiftResponse) or a ServiceTitanError.</returns>
            Public Shared Function getTechnicianShift(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/dispatch/v2/tenant/" & tenant & "/technician-shifts/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As TechnicianShiftResponse = JsonConvert.DeserializeObject(Of TechnicianShiftResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class EquipmentSystems
            Inherits Types.EquipmentSystems
            ''' <summary>
            ''' Creates a new Installed Equipment on a location ID.
            ''' </summary>
            ''' <param name="equipment">Information to add the new equipment to the file.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns an instance of the new installed equipment (InstalledEquipmentDetailedResponse), or a ServiceTitanError.</returns>
            Public Shared Function createInstalledEquipment(ByVal equipment As InstalledEquipmentCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object
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
                    Dim baseurl As String = domain & "/equipmentsystems/v2/tenant/" & tenant & "/installed-equipment"



                    Console.WriteLine("Executing: " & baseurl)
                    Dim req As WebRequest = WebRequest.Create(baseurl)
                    req.Method = "POST"
                    req.Timeout = 999999

                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)

                    Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(equipment))
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

                    Dim deserialized As InstalledEquipmentDetailedResponse = JsonConvert.DeserializeObject(Of InstalledEquipmentDetailedResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Return deserialized
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Returns a single installed equipment by it's ID.
            ''' </summary>
            ''' <param name="id">The installed equipment ID.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either an installed equipment (InstalledEquipmentDetailedResponse) or a ServiceTitanError.</returns>
            Public Shared Function getInstalledEquipment(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/equipmentsystems/v2/tenant/" & tenant & "/installed-equipment/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As InstalledEquipmentDetailedResponse = JsonConvert.DeserializeObject(Of InstalledEquipmentDetailedResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Returns a paginated list of Installed Equipments.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of filters to limit the list of returned records</param>
            ''' <returns>Returns either a paginated list of installed equipments (InstalledEquipmentResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getInstalledEquipments(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/equipmentsystems/v2/tenant/" & tenant & "/installed-equipment"

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
                    req.ContentType = "application/json"

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
                    Dim results As InstalledEquipmentResponse_P = JsonConvert.DeserializeObject(Of InstalledEquipmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an existing installed equipment record, by it's ID.
            ''' </summary>
            ''' <param name="id">The installed equipment ID</param>
            ''' <param name="equipment">The information to update on the equipment.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, the request returns the updated equipment (InstalledEquipmentDetailedResponse) or a ServiceTitanError.</returns>
            Public Shared Function UpdateInstalledEquipment(ByVal id As Integer, ByVal equipment As InstalledEquipmentUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)
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
                    Dim baseurl As String = domain & "/equipmentsystems/v2/tenant/" & tenant & "/installed-equipment/" & id



                    Console.WriteLine("Executing: " & baseurl)
                    Dim req As WebRequest = WebRequest.Create(baseurl)
                    req.Method = "PATCH"
                    req.Timeout = 999999

                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)

                    Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(equipment))
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
                    Dim results As InstalledEquipmentDetailedResponse = JsonConvert.DeserializeObject(Of InstalledEquipmentDetailedResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class Inventory
            Inherits Types.Inventory
            ''' <summary>
            ''' Creates a new purchase order, using a CreatePurchaseOrderRequest payload.
            ''' </summary>
            ''' <param name="purchaseorder">The new purchase order to create</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, the request returns the created equipment's ID as a CreatePurchaseOrderResponse class, or a ServiceTitanError.</returns>
            Public Shared Function createPurchaseOrder(ByVal purchaseorder As CreatePurchaseOrderRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object
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
                    Dim baseurl As String = domain & "/inventory/v2/tenant/" & tenant & "/purchase-orders"



                    Console.WriteLine("Executing: " & baseurl)
                    Dim req As WebRequest = WebRequest.Create(baseurl)
                    req.Method = "POST"
                    req.Timeout = 999999

                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)

                    Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(purchaseorder))
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

                    Dim deserialized As CreatePurchaseOrderResponse = JsonConvert.DeserializeObject(Of CreatePurchaseOrderResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Return deserialized
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Queries for a list of purchase orders, using a list of OptionsList as filters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of filters, as well as options to cycle through pages.</param>
            ''' <returns>A paginated list of purchase orders (as PurchaseOrderResponse_P), or a ServiceTitanError.</returns>
            Public Shared Function getPurchaseOrders(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/inventory/v2/tenant/" & tenant & "/purchase-orders"

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
                    req.ContentType = "application/json"

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
                    Dim results As PurchaseOrderResponse_P = JsonConvert.DeserializeObject(Of PurchaseOrderResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single Purchase Order by it's ID.
            ''' </summary>
            ''' <param name="id">Purchase Order ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns a PurchaseOrderResponse, or a ServiceTitanError.</returns>
            Public Shared Function getPurchaseOrder(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/inventory/v2/tenant/" & tenant & "/purchase-orders"

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
                    req.ContentType = "application/json"

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
                    Dim results As PurchaseOrderResponse = JsonConvert.DeserializeObject(Of PurchaseOrderResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an existing purchase order
            ''' </summary>
            ''' <param name="poId">The Purchase Order ID to update.</param>
            ''' <param name="purchaseorder">The updated purchase order</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns an instance of CreatePurchaseOrderResponse, or a ServiceTitanError</returns>
            Public Shared Function UpdatePurchaseOrder(ByVal poId As Integer, ByVal purchaseorder As UpdatePurchaseOrderRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)
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
                    Dim baseurl As String = domain & "/inventory/v2/tenant/" & tenant & "/purchase-orders/" & poId



                    Console.WriteLine("Executing: " & baseurl)
                    Dim req As WebRequest = WebRequest.Create(baseurl)
                    req.Method = "PATCH"
                    req.Timeout = 999999

                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)

                    Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(purchaseorder))
                    req.ContentLength = bytearray.Length
                    req.Timeout = 999999
                    Dim datastream As Stream = req.GetRequestStream
                    datastream.Write(bytearray, 0, bytearray.Length)
                    datastream.Close()

                    Dim response As WebResponse = req.GetResponse
                    Dim buffer As Stream = response.GetResponseStream
                    Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                    Dim output As String = streamread.ReadToEnd
                    Dim results As CreatePurchaseOrderResponse = JsonConvert.DeserializeObject(Of CreatePurchaseOrderResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class JobBooking_ContactExperience
            Inherits Types.JobBooking_ContactExperience
            ''' <summary>
            ''' Gets a list of call reasons, depending on the provided OptionsList.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A filter list for this request</param>
            ''' <returns>Returns either a paginated list of CallReasonResponse (CallReasonResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getCallReasons(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jbce/v2/tenant/" & tenant & "/call-reasons"

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
                    req.ContentType = "application/json"

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
                    Dim results As CallReasonResponse_P = JsonConvert.DeserializeObject(Of CallReasonResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class JobPlanning_Management
            Inherits Types.JobPlanning_Management
            ''' <summary>
            ''' Creates a new appointment onto an existing job.
            ''' </summary>
            ''' <param name="appointment">The new appointment to create</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either the created appointment (AppointmentResponse) or a ServiceTitanError.</returns>
            Public Shared Function createAppointment(ByVal appointment As AppointmentAddRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object
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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/appointments"



                    Console.WriteLine("Executing: " & baseurl)
                    Dim req As WebRequest = WebRequest.Create(baseurl)
                    req.Method = "POST"
                    req.Timeout = 999999

                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)

                    Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(appointment))
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

                    Dim deserialized As AppointmentResponse = JsonConvert.DeserializeObject(Of AppointmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Return deserialized
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates the special instructions on an existing appointment.
            ''' </summary>
            ''' <param name="id">The appointment ID</param>
            ''' <param name="instructions">The special instructions</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either the updated appointment (AppointmentResponse) or a ServiceTitanError.</returns>
            Public Shared Function updateAppointmentSpecialInstructions(ByVal id As Integer, ByVal instructions As UpdateAppointmentSpecialInstructionsRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object
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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/appointments/" & id & "/special-instructions"



                    Console.WriteLine("Executing: " & baseurl)
                    Dim req As WebRequest = WebRequest.Create(baseurl)
                    req.Method = "PUT"
                    req.Timeout = 999999

                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)

                    Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(instructions))
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

                    Dim deserialized As AppointmentResponse = JsonConvert.DeserializeObject(Of AppointmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Return deserialized
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Deletes an appointment.
            ''' </summary>
            ''' <param name="id">The appointment's ID.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, this function returns Nothing, or a ServiceTitanError.</returns>
            Public Shared Function deleteAppointment(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object
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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/appointments/" & id



                    Console.WriteLine("Executing: " & baseurl)
                    Dim req As WebRequest = WebRequest.Create(baseurl)
                    req.Method = "DEL"
                    req.Timeout = 999999

                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)

                    'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(instructions))
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

                    'Dim deserialized As AppointmentResponse = JsonConvert.DeserializeObject(Of AppointmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Return Nothing
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a paginated list of appointments, based on the specified OptionsList.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">The options list for this parameter (Filters)</param>
            ''' <returns>Returns either a paginated list of AppointmentResponse (AppointmentResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getAppointments(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/appointments"

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
                    req.ContentType = "application/json"

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
                    Dim results As AppointmentResponse_P = JsonConvert.DeserializeObject(Of AppointmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single appointment, by it's ID
            ''' </summary>
            ''' <param name="id">The appointment's ID.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a AppointmentResponse, or a ServiceTitanError.</returns>
            Public Shared Function getAppointment(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/appointments/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As AppointmentResponse = JsonConvert.DeserializeObject(Of AppointmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Puts an appointment on hold.
            ''' </summary>
            ''' <param name="id">The appointment ID</param>
            ''' <param name="reason">The reason for putting the appointment on hold</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, the function returns Nothing. If not, it returns a ServiceTitanError.</returns>
            Public Shared Function putAppointmentOnHold(ByVal id As Integer, ByVal reason As HoldAppointmentRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/appointments/" & id & "/hold"

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
                    req.Method = "PUT"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

                    Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reason))
                    req.ContentLength = bytearray.Length
                    req.Timeout = 999999
                    Dim datastream As Stream = req.GetRequestStream
                    datastream.Write(bytearray, 0, bytearray.Length)
                    datastream.Close()

                    Dim response As WebResponse = req.GetResponse
                    Dim buffer As Stream = response.GetResponseStream
                    Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                    Dim output As String = streamread.ReadToEnd
                    'Dim results As AppointmentResponse_P = JsonConvert.DeserializeObject(Of AppointmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return Nothing
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Removes hold from the appointment.
            ''' </summary>
            ''' <param name="id">The appointment ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, the function returns Nothing. If not, it returns a ServiceTitanError.</returns>

            Public Shared Function removeAppointmentFromHold(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/appointments/" & id & "/hold"

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
                    req.Method = "DEL"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

                    'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reason))
                    'req.ContentLength = bytearray.Length
                    'req.Timeout = 999999
                    'Dim datastream As Stream = req.GetRequestStream
                    'datastream.Write(bytearray, 0, bytearray.Length)
                    'datastream.Close()

                    Dim response As WebResponse = req.GetResponse
                    Dim buffer As Stream = response.GetResponseStream
                    Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                    Dim output As String = streamread.ReadToEnd
                    'Dim results As AppointmentResponse_P = JsonConvert.DeserializeObject(Of AppointmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return Nothing
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Reschedules a single appointment.
            ''' </summary>
            ''' <param name="id">The appointment's ID.</param>
            ''' <param name="reschedulerequest">The reschedule request</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, returns Nothing. If the request fails, returns a ServiceTitanError.</returns>
            Public Shared Function rescheduleAppointment(ByVal id As Integer, ByVal reschedulerequest As AppointmentRescheduleRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/appointments/" & id & "/reschedule"

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

                    Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(reschedulerequest))
                    req.ContentLength = bytearray.Length
                    req.Timeout = 999999
                    Dim datastream As Stream = req.GetRequestStream
                    datastream.Write(bytearray, 0, bytearray.Length)
                    datastream.Close()

                    Dim response As WebResponse = req.GetResponse
                    Dim buffer As Stream = response.GetResponseStream
                    Dim streamread As StreamReader = New StreamReader(buffer, Text.Encoding.UTF8)
                    Dim output As String = streamread.ReadToEnd
                    'Dim results As AppointmentResponse_P = JsonConvert.DeserializeObject(Of AppointmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return Nothing
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of Job Cancel Reasons, in a paginated format.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/filters for this request</param>
            ''' <returns>Returns either a paginated list of Job Cancel Reasons (JobCancelReasonResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getJobCancelReasonsList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/job-cancel-reasons"

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
                    req.ContentType = "application/json"

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
                    Dim results As JobCancelReasonResponse_P = JsonConvert.DeserializeObject(Of JobCancelReasonResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of Job on Hold Reasons, in a paginated format.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/filters for this request</param>
            ''' <returns>Returns either a paginated list of Job On Hold Reasons (JobHoldReasonResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getJobHoldReasons(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/job-hold-reasons"

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
                    req.ContentType = "application/json"

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
                    Dim results As JobHoldReasonResponse_P = JsonConvert.DeserializeObject(Of JobHoldReasonResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Cancels the specified job, for the specified reason.
            ''' </summary>
            ''' <param name="jobid">The Job ID to cancel</param>
            ''' <param name="payload">The payload, containing the reason for cancellation.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, request returns Nothing. If error, returns a ServiceTitanError.</returns>
            Public Shared Function cancelJob(ByVal jobid As Integer, ByVal payload As CancelJobRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/jobs/" & jobid & "/cancel"

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
                    req.Method = "PUT"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As JobHoldReasonResponse_P = JsonConvert.DeserializeObject(Of JobHoldReasonResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return Nothing
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a new job
            ''' </summary>
            ''' <param name="payload">The data to put in the new job</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a JobResponse, or a ServiceTitanError.</returns>
            Public Shared Function createJob(ByVal payload As JobCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/jobs/"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As JobResponse = JsonConvert.DeserializeObject(Of JobResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a new note on a job.
            ''' </summary>
            ''' <param name="jobid">The job ID where the note should appear.</param>
            ''' <param name="payload">The note payload</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a NoteResponse, or a ServiceTitanError.</returns>
            Public Shared Function createJobNote(ByVal jobid As Integer, ByVal payload As JobNoteCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/jobs/" & jobid & "/notes"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As NoteResponse = JsonConvert.DeserializeObject(Of NoteResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single job, by it's ID.
            ''' </summary>
            ''' <param name="jobid">The job ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of OptionList. Right now, the only option available for this API endpoint is externalDataApplicationGuid.</param>
            ''' <returns>Either returns a JobResponse, or a ServiceTitanError.</returns>
            Public Shared Function getJob(ByVal jobid As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/jobs/" & jobid

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
                    req.ContentType = "application/json"

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
                    Dim results As JobResponse = JsonConvert.DeserializeObject(Of JobResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of Cancel Reasons for a job.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/filters.</param>
            ''' <returns>Either returns a paginated list of Cancel reasons (CancelReasonResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getJobCancelReasons(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/jobs/cancel-reasons"

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
                    req.ContentType = "application/json"

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
                    Dim results As CancelReasonResponse_P = JsonConvert.DeserializeObject(Of CancelReasonResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Returns a job's full history.
            ''' </summary>
            ''' <param name="jobid">The job ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a JobHistoryResponse, or a ServiceTitanError.</returns>
            Public Shared Function getJobHistory(ByVal jobid As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/jobs/" & jobid & "/history"

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
                    req.ContentType = "application/json"

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
                    Dim results As JobHistoryResponse = JsonConvert.DeserializeObject(Of JobHistoryResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of jobs (Based on your filters/options)
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/filters for this endpoint.</param>
            ''' <returns>Either returns a paginated list of jobs (JobResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getJobList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/jobs/"

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
                    req.ContentType = "application/json"

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
                    Dim results As JobResponse_P = JsonConvert.DeserializeObject(Of JobResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a paginated list of a job's notes.
            ''' </summary>
            ''' <param name="jobid">The job's ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/filters for this endpoint.</param>
            ''' <returns>Returns either a paginated list of notes (NoteResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getJobNotes(ByVal jobid As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/jobs/" & jobid & "/notes"

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
                    req.ContentType = "application/json"

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
                    Dim results As NoteResponse_P = JsonConvert.DeserializeObject(Of NoteResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Puts a job on hold. Requires a reason.
            ''' </summary>
            ''' <param name="jobid">Job ID</param>
            ''' <param name="payload">The reason</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns Nothing (if successful) or a ServiceTitanError.</returns>
            Public Shared Function putJobOnHold(ByVal jobid As Integer, ByVal payload As HoldJobRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/jobs/" & jobid & "/hold"

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
                    req.Method = "PUT"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As NoteResponse_P = JsonConvert.DeserializeObject(Of NoteResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return Nothing
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates a job with new information.
            ''' </summary>
            ''' <param name="jobid">Job's ID</param>
            ''' <param name="payload">The updated job information.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a JobResponse, or a ServiceTitanError.</returns>
            Public Shared Function updateJob(ByVal jobid As Integer, ByVal payload As UpdateJobRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/jobs/" & jobid

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As JobResponse = JsonConvert.DeserializeObject(Of JobResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates a single job type with new information.
            ''' </summary>
            ''' <param name="jobTypeID">The job type's ID.</param>
            ''' <param name="payload">The job type payload</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a JobTypeResponse, or a ServiceTitanError.</returns>
            Public Shared Function updateJobType(ByVal jobTypeID As Integer, ByVal payload As UpdateJobTypeRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/job-types/" & jobTypeID

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As JobTypeResponse = JsonConvert.DeserializeObject(Of JobTypeResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a new job type.
            ''' </summary>
            ''' <param name="payload">The job type to create</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a JobTypeResponse, or a ServiceTitanError.</returns>
            Public Shared Function createJobType(ByVal payload As CreateJobTypeRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/job-types"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As JobTypeResponse = JsonConvert.DeserializeObject(Of JobTypeResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single job type, by it's ID.
            ''' </summary>
            ''' <param name="jobTypeID">The job type's ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/filters for this endpoint.</param>
            ''' <returns>Either returns a JobTypeResponse, or a ServiceTitanError.</returns>
            Public Shared Function getJobType(ByVal jobTypeID As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/job-types/" & jobTypeID

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
                    req.ContentType = "application/json"

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
                    Dim results As JobTypeResponse = JsonConvert.DeserializeObject(Of JobTypeResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of job types, based on your options/filters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/filters for this endpoint.</param>
            ''' <returns>Either returns a paginated list of job types (JobTypeResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getJobTypeList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/job-types"

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
                    req.ContentType = "application/json"

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
                    Dim results As JobTypeResponse_P = JsonConvert.DeserializeObject(Of JobTypeResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of projects, based on your options/filters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/filters for this endpoint.</param>
            ''' <returns>Either returns a paginated list of projects (ProjectResponse_P) or a ServiceTitanError.</returns>

            Public Shared Function getProjectList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/projects"

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
                    req.ContentType = "application/json"

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
                    Dim results As ProjectResponse_P = JsonConvert.DeserializeObject(Of ProjectResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single project, by it's ID.
            ''' </summary>
            ''' <param name="projectID">The project's ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a ProjectResponse, or a ServiceTitanError.</returns>
            Public Shared Function getProject(ByVal projectID As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/jpm/v2/tenant/" & tenant & "/projects/" & projectID

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
                    req.ContentType = "application/json"

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
                    Dim results As ProjectResponse = JsonConvert.DeserializeObject(Of ProjectResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class Marketing
            Inherits Types.Marketing
            ''' <summary>
            ''' Creates a new campaign category
            ''' </summary>
            ''' <param name="payload">The category payload</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function createCampaignCategories(ByVal payload As CampaignCategoryCreateUpdateModel, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/categories"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As ProjectResponse = JsonConvert.DeserializeObject(Of ProjectResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return Nothing
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of Campaign Categories (Not paginated).
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a list of Campaigns (List Of(CampaignCategoryModel)) or a ServiceTitanError.</returns>
            Public Shared Function getCampaignCategoriesList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/categories"

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
                    req.ContentType = "application/json"

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
                    Dim results As List(Of CampaignCategoryModel) = JsonConvert.DeserializeObject(Of List(Of CampaignCategoryModel))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single Campaign Category.
            ''' </summary>
            ''' <param name="id">The Campaign Category ID.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a CampaignCategoryModel, or a ServiceTitanError.</returns>
            Public Shared Function getCampaignCategory(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/categories/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As CampaignCategoryModel = JsonConvert.DeserializeObject(Of CampaignCategoryModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates a single campaign category by it's ID.
            ''' </summary>
            ''' <param name="id">The campaign category id.</param>
            ''' <param name="payload">The updated campaign category</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, returns Nothing. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function updateCampaignCategory(ByVal id As Integer, ByVal payload As CampaignCategoryCreateUpdateModel, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/categories/" & id

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
                    req.ContentType = "application/json"

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
                    'Dim results As CampaignCategoryModel = JsonConvert.DeserializeObject(Of CampaignCategoryModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return Nothing
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a new campaign cost record.
            ''' </summary>
            ''' <param name="payload">The new campaign cost</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, returns a CampaignCostModel. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function createCampaignCost(ByVal payload As CreateCostRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/costs"

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
                    req.ContentType = "application/json"

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
                    Dim results As CampaignCostModel = JsonConvert.DeserializeObject(Of CampaignCostModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an existing Campaign Cost.
            ''' </summary>
            ''' <param name="payload">The updated campaign cost.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, returns Nothing. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function updateCampaignCost(ByVal payload As UpdateCostRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/costs"

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As CampaignCostModel = JsonConvert.DeserializeObject(Of CampaignCostModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return Nothing
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a new marketing campaign.
            ''' </summary>
            ''' <param name="payload">The campaign to create</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a DetailedCampaignModel (if successful), or a ServiceTitanError.</returns>
            Public Shared Function createCampaign(ByVal payload As CampaignCreateModel, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/campaigns"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As DetailedCampaignModel = JsonConvert.DeserializeObject(Of DetailedCampaignModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of marketing campaigns, based on your parameters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Returns either a paginated list of campaigns (CampaignModel_P) or a ServiceTitanError.</returns>
            Public Shared Function getCampaignList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/campaigns"

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
                    req.ContentType = "application/json"

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
                    Dim results As CampaignModel_P = JsonConvert.DeserializeObject(Of CampaignModel_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a campaign's costs.
            ''' </summary>
            ''' <param name="campaignId">The campaign ID to get the costs for.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Returns either a list of CampaignCostModel, or a ServiceTitanError.</returns>
            Public Shared Function getCampaignCosts(ByVal campaignId As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/campaigns/" & campaignId & "/costs"

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
                    req.ContentType = "application/json"

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
                    Dim results As List(Of CampaignCostModel) = JsonConvert.DeserializeObject(Of List(Of CampaignCostModel))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single campaign, by it's ID.
            ''' </summary>
            ''' <param name="campaignId">The campaign's ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a single campaign (as DetailedCampaignModel) or a ServiceTitanError.</returns>
            Public Shared Function GetCampaign(ByVal campaignId As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/campaigns/" & campaignId

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
                    req.ContentType = "application/json"

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
                    Dim results As DetailedCampaignModel = JsonConvert.DeserializeObject(Of DetailedCampaignModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates a campaign.
            ''' </summary>
            ''' <param name="campaignId">The campaign ID</param>
            ''' <param name="payload">The campaign update class</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a DetailedCampaignModel, or a ServiceTitanError.</returns>
            Public Shared Function updateCampaign(ByVal campaignId As Integer, ByVal payload As CampaignCreateModel, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/marketing/v2/tenant/" & tenant & "/campaigns/" & campaignId

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
                    req.Method = "PUT"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As DetailedCampaignModel = JsonConvert.DeserializeObject(Of DetailedCampaignModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class Memberships
            Inherits Types.Memberships
            ''' <summary>
            ''' Creates a membership sale invoice, thus also creating the membership. This endpoint is the equivalent of clicking the 'Sell Membership' button in the location page in the GUI.
            ''' </summary>
            ''' <param name="payload">The new membership sale</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a MembershipSaleInvoiceCreateResponse, or a ServiceTitanError.</returns>
            Public Shared Function sellMembership(ByVal payload As MembershipSaleInvoiceCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/memberships/sale"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As MembershipSaleInvoiceCreateResponse = JsonConvert.DeserializeObject(Of MembershipSaleInvoiceCreateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of customer memberships, using options as a filter.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">List of options/filters</param>
            ''' <returns>Either returns a paginated list of memberships (CustomerMembershipResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getMembershipList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/memberships"

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
                    req.ContentType = "application/json"

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
                    Dim results As CustomerMembershipResponse_P = JsonConvert.DeserializeObject(Of CustomerMembershipResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a membership record by it's ID.
            ''' </summary>
            ''' <param name="id">The membership ID.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns a single membership (CustomerMembershipResponse) or a ServiceTitanError.</returns>
            Public Shared Function getMembership(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/memberships/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As CustomerMembershipResponse = JsonConvert.DeserializeObject(Of CustomerMembershipResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates a membership with new information.
            ''' </summary>
            ''' <param name="id">The membership's ID.</param>
            ''' <param name="payload">The new membership information</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a ModificationResponse (If successful), or a ServiceTitanError.</returns>
            Public Shared Function updateMembership(ByVal id As Integer, ByVal payload As CustomerMembershipUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/memberships/" & id

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a new invoice template.
            ''' </summary>
            ''' <param name="payload">The new invoice template</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns a ModificationResponse (if successful) or a ServiceTitanError.</returns>
            Public Shared Function createInvoiceTemplate(ByVal payload As InvoiceTemplateCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/invoice-templates"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets invoice templates using a list of IDs (Max: 50).
            ''' </summary>
            ''' <param name="ids">A list of invoice template IDs. (TODO: Find if they need to be comma-delimited, assuming yes)</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a paginated list of Invoice templates (InvoiceTemplateResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getInvoiceTemplatesByIds(ByVal ids As String, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/invoice-templates?ids=" & System.Net.WebUtility.UrlEncode(ids)

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
                    req.ContentType = "application/json"

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
                    Dim results As InvoiceTemplateResponse_P = JsonConvert.DeserializeObject(Of InvoiceTemplateResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single invoice template by it's ID.
            ''' </summary>
            ''' <param name="id">The Invoice Template ID.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns an InvoiceTemplateResponse, or a ServiceTitanError.</returns>
            Public Shared Function getInvoiceTemplate(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/invoice-templates/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As InvoiceTemplateResponse = JsonConvert.DeserializeObject(Of InvoiceTemplateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates a single invoice template.
            ''' </summary>
            ''' <param name="id">The Invoice Template ID</param>
            ''' <param name="payload">The Invoice Template Payload</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a ModificationResponse, or a ServiceTitanError.</returns>
            Public Shared Function updateInvoiceTemplate(ByVal id As Integer, ByVal payload As InvoiceTemplateUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/invoice-templates/" & id

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Returns a list of Recurring Service Events based on your parameters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of filters/parameters</param>
            ''' <returns>Either returns a paginated list of Recurring Service Events (LocationRecurringServiceEventResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getRecurringServiceEvents(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/invoice-templates"

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
                    req.ContentType = "application/json"

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
                    Dim results As LocationRecurringServiceEventResponse_P = JsonConvert.DeserializeObject(Of LocationRecurringServiceEventResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Marking an event as complete links the job with provided JobID to the given Location Recurring Service Event. It will also copy over invoice items to the Job Invoice corresponding to the Invoice Template of the Location Recurring Service the Event was generated from.
            ''' </summary>
            ''' <param name="id">The recurring service event id.</param>
            ''' <param name="payload">The payload (Containing the job id)</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, returns a ModificationResponse. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function markRecurringServiceEventComplete(ByVal id As Integer, ByVal payload As MarkEventCompletedStatusUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/invoice-templates/" & id & "/mark-complete"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Marking an event as incomplete unlinks the job with provided JobID to the given Location Recurring Service Event. It will also delete the invoice items that were copied over when the Location Recurring Service Event was marked as completed on the Job.
            ''' </summary>
            ''' <param name="id">The recurring service event id.</param>
            ''' <param name="payload">The payload (Containing the job id)</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, returns a ModificationResponse. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function markRecurringServiceEventIncomplete(ByVal id As Integer, ByVal payload As MarkEventCompletedStatusUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/invoice-templates/" & id & "/mark-incomplete"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of recurring services.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">List of properties/filters</param>
            ''' <returns>Either returns a paginated list of LocationRecurringServiceResponse (LocationRecurringServiceResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getRecurringServicesList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/recurring-services"

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
                    req.ContentType = "application/json"

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
                    Dim results As LocationRecurringServiceResponse_P = JsonConvert.DeserializeObject(Of LocationRecurringServiceResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Returns a single recurring service (By ID).
            ''' </summary>
            ''' <param name="id">The recurring service ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns a LocationRecurringServiceResponse, or a ServiceTitanError.</returns>
            Public Shared Function getRecurringService(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/recurring-services/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As LocationRecurringServiceResponse = JsonConvert.DeserializeObject(Of LocationRecurringServiceResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates a single recurring service (By ID).
            ''' </summary>
            ''' <param name="id">The recurring service ID</param>
            ''' <param name="payload">The update payload</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a ModificationResponse (If successful) or a ServiceTitanError.</returns>
            Public Shared Function updateRecurringService(ByVal id As Integer, ByVal payload As LocationRecurringServiceUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/recurring-services/" & id

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of membership types.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters for the API.</param>
            ''' <returns>Returns either a list of membership types (MembershipTypeResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getMembershipTypesList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/membership-types"

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
                    req.ContentType = "application/json"

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
                    Dim results As MembershipTypeResponse_P = JsonConvert.DeserializeObject(Of MembershipTypeResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets discounts for the given membership type
            ''' </summary>
            ''' <param name="id">Membership type ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a list of Membership Type Discounts (List of(MembershipTypeDiscountItemResponse)) or a ServiceTitanError.</returns>
            Public Shared Function getMembershipTypeDiscounts(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/membership-types/" & id & "/discounts"

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
                    req.ContentType = "application/json"

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
                    Dim results As List(Of MembershipTypeDiscountItemResponse) = JsonConvert.DeserializeObject(Of List(Of MembershipTypeDiscountItemResponse))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets duration/billing options for the given membership type
            ''' </summary>
            ''' <param name="id">The membership Type ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/parameters (For this endpoint, only 'active' is accepted)</param>
            ''' <returns>Returns either a 'List(Of MembershipTypeDurationBillingItemResponse)' or a ServiceTitanError.</returns>
            Public Shared Function getMembershipTypeDurationBillingItems(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/membership-types/" & id & "/duration-billing-items"

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
                    req.ContentType = "application/json"

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
                    Dim results As List(Of MembershipTypeDurationBillingItemResponse) = JsonConvert.DeserializeObject(Of List(Of MembershipTypeDurationBillingItemResponse))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single membership type (By it's ID)
            ''' </summary>
            ''' <param name="id">The membership type's ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a MembershipTypeResponse, or a ServiceTitanError.</returns>
            Public Shared Function getMembershipType(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/membership-types/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As MembershipTypeResponse = JsonConvert.DeserializeObject(Of MembershipTypeResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of Service Items for a specific Membership Type's Recurring event.
            ''' </summary>
            ''' <param name="id">The membership type ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a 'List (Of MembershipTypeRecurringServiceItemResponse)' or a ServiceTitanError.</returns>
            Public Shared Function getMembershipTypeRecurringServiceItems(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/membership-types/" & id & "/recurring-service-items"

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
                    req.ContentType = "application/json"

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
                    Dim results As List(Of MembershipTypeRecurringServiceItemResponse) = JsonConvert.DeserializeObject(Of List(Of MembershipTypeRecurringServiceItemResponse))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a paginated list of Recurring Service Types.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/parameters</param>
            ''' <returns>Either returns a paginated list of recurring service types (RecurringServiceTypeResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getRecurringServiceTypeList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/recurring-service-types"

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
                    req.ContentType = "application/json"

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
                    Dim results As RecurringServiceTypeResponse_P = JsonConvert.DeserializeObject(Of RecurringServiceTypeResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single Recurring Service Type by it's ID.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="id">The recurring service type's ID.</param>
            ''' <returns>Either returns a RecurringServiceTypeResponse or a ServiceTitanError.</returns>

            Public Shared Function getRecurringServiceType(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/memberships/v2/tenant/" & tenant & "/recurring-service-types/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As RecurringServiceTypeResponse = JsonConvert.DeserializeObject(Of RecurringServiceTypeResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class Payroll
            Inherits Types.Payroll
            ''' <summary>
            ''' Gets a list of activity codes.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of options/parameters.</param>
            ''' <returns>Either returns a paginated list of activity codes (PayrollActivityCodeResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getActivityCodeList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/activity-codes"

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
                    req.ContentType = "application/json"

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
                    Dim results As PayrollActivityCodeResponse_P = JsonConvert.DeserializeObject(Of PayrollActivityCodeResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single activity code, by it's ID.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="id">The activity code ID</param>
            ''' <returns>Either returns a single activity code (PayrollActivityCodeResponse) or a ServiceTitanError.</returns>

            Public Shared Function getActivityCode(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/activity-codes/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As PayrollActivityCodeResponse = JsonConvert.DeserializeObject(Of PayrollActivityCodeResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a new Gross Pay Item record.
            ''' </summary>
            ''' <param name="payload">The gross pay item create request</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns the ID of the created item (In the form of a ModificationResponse) or a ServiceTitanError.</returns>
            Public Shared Function createGrossPayItem(ByVal payload As GrossPayItemCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/gross-pay-items"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Deletes a specific Gross Pay Item.
            ''' </summary>
            ''' <param name="id">The Gross Pay Item's ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns nothing (If successful) or returns a ServiceTitanError.</returns>
            Public Shared Function deleteGrossPayItem(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/gross-pay-items/" & id

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
                    req.Method = "DEL"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of gross pay items matching what is requested.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of filters/parameters</param>
            ''' <returns>Either returns a paginated list of gross pay items (GrossPayItemResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getGrossPayItemList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/gross-pay-items"

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
                    req.ContentType = "application/json"

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
                    Dim results As GrossPayItemResponse_P = JsonConvert.DeserializeObject(Of GrossPayItemResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an existing Gross Pay Item record
            ''' </summary>
            ''' <param name="payload">The updated Gross Pay Item</param>
            ''' <param name="id">The Gross Pay Item ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>If successful, returns a ModificationResponse. If not, returns a ServiceTitanError.</returns>
            Public Shared Function updateGrossPayItem(ByVal payload As GrossPayItemCreateRequest, ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/gross-pay-items/" & id

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
                    req.Method = "PUT"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})

                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of splits for a specific job id.
            ''' </summary>
            ''' <param name="id">The job ID.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a list of JobSplitResponse, or a ServiceTitanError.</returns>
            Public Shared Function getJobSplits(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/jobs/" & id & "/splits"

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
                    req.ContentType = "application/json"

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
                    Dim results As List(Of JobSplitResponse) = JsonConvert.DeserializeObject(Of List(Of JobSplitResponse))(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a new payroll adjustment.
            ''' </summary>
            ''' <param name="payload">The payroll adjustment</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a ModificationResponse or a ServiceTitanError.</returns>
            Public Shared Function createPayrollAdjustment(ByVal payload As PayrollAdjustmentCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/payroll-adjustments"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ModificationResponse = JsonConvert.DeserializeObject(Of ModificationResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of payroll adjustments, based on your parameters/filters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Either returns a list of payroll adjustments (PayrollAdjustmentResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getPayrollAdjustmentList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/payroll-adjustments"

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
                    req.ContentType = "application/json"

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
                    Dim results As PayrollAdjustmentResponse_P = JsonConvert.DeserializeObject(Of PayrollAdjustmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a specific payroll adjustment record, by it's ID.
            ''' </summary>
            ''' <param name="id">The payroll adjustment ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a PayrollAdjustmentResponse, or a ServiceTitanError.</returns>
            Public Shared Function getPayrollAdjustment(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/payroll-adjustments/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As PayrollAdjustmentResponse = JsonConvert.DeserializeObject(Of PayrollAdjustmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets an employee's payroll records, based on the employee's ID.
            ''' </summary>
            ''' <param name="id">The employee's ID.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Either returns a paginated list of Payroll records (PayrollResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getEmployeePayrollRecords(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/employees/" & id & "/payrolls"

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
                    req.ContentType = "application/json"

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
                    Dim results As PayrollResponse_P = JsonConvert.DeserializeObject(Of PayrollResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets payroll records.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Either returns a paginated list of Payroll records (PayrollResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getPayrollRecords(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/payrolls"

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
                    req.ContentType = "application/json"

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
                    Dim results As PayrollResponse_P = JsonConvert.DeserializeObject(Of PayrollResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a technician's payroll records, based on the employee's ID.
            ''' </summary>
            ''' <param name="id">The technician's ID.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Either returns a paginated list of Payroll records (PayrollResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getTechnicianPayrollRecords(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/technicians/" & id & "/payrolls"

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
                    req.ContentType = "application/json"

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
                    Dim results As PayrollResponse_P = JsonConvert.DeserializeObject(Of PayrollResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of timesheet codes
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters.</param>
            ''' <returns>Either returns a paginated list of timesheet codes (TimesheetCodeResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getTimesheetCodes(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/timesheet-codes"

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
                    req.ContentType = "application/json"

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
                    Dim results As TimesheetCodeResponse_P = JsonConvert.DeserializeObject(Of TimesheetCodeResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a specific timesheet code, by it's ID.
            ''' </summary>
            ''' <param name="id">The timesheet code ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a TimesheetCodeResponse or a ServiceTitanError.</returns>
            Public Shared Function getTimesheetCode(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/payroll/v2/tenant/" & tenant & "/timesheet-codes/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As TimesheetCodeResponse = JsonConvert.DeserializeObject(Of TimesheetCodeResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class Pricebook
            Inherits Types.Pricebook
            ''' <summary>
            ''' Creates a new category in the pricebook.
            ''' </summary>
            ''' <param name="payload">The new category to add</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either the created category (CategoryResponse) or a ServiceTitanError.</returns>
            Public Shared Function createCategory(ByVal payload As CategoryCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/categories"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As CategoryResponse = JsonConvert.DeserializeObject(Of CategoryResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Deletes a category from the pricebook using it's ID.
            ''' </summary>
            ''' <param name="id">The category ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>IF successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function deleteCategory(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/categories/" & id

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
                    req.Method = "DEL"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As CategoryResponse = JsonConvert.DeserializeObject(Of CategoryResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a category, using it's ID
            ''' </summary>
            ''' <param name="id">The category ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a CategoryResponse, or a ServiceTitanError.</returns>
            Public Shared Function getCategory(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/categories/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As CategoryResponse = JsonConvert.DeserializeObject(Of CategoryResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of categories, depending on your parameters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/options</param>
            ''' <returns>Either returns a paginated list of categories (CategoryResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getCategoryList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/categories"

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
                    req.ContentType = "application/json"

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
                    Dim results As CategoryResponse_P = JsonConvert.DeserializeObject(Of CategoryResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an existing category
            ''' </summary>
            ''' <param name="id">The category's ID</param>
            ''' <param name="payload">The updated category</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a CategoryResponse, or a ServiceTitanError.</returns>
            Public Shared Function updateCategory(ByVal id As Integer, ByVal payload As CategoryUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/categories/" & id

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As CategoryResponse = JsonConvert.DeserializeObject(Of CategoryResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function

            ''' <summary>
            ''' Creates a new discount and/or fee in the pricebook.
            ''' </summary>
            ''' <param name="payload">The new discount and/or fee to add</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either the created discount and/or fee (DiscountAndFeesResponse) or a ServiceTitanError.</returns>
            Public Shared Function createDiscountAndFees(ByVal payload As DiscountAndFeesCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/discounts-and-fees"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As DiscountAndFeesResponse = JsonConvert.DeserializeObject(Of DiscountAndFeesResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Deletes a discount and/or fee from the pricebook using it's ID.
            ''' </summary>
            ''' <param name="id">The discount and/or fee ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>IF successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function deleteDiscountAndFees(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/discounts-and-fees/" & id

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
                    req.Method = "DEL"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As DiscountAndFeesResponse = JsonConvert.DeserializeObject(Of DiscountAndFeesResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a discount and/or fee, using it's ID
            ''' </summary>
            ''' <param name="id">The discount and/or fee ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a DiscountAndFeesResponse, or a ServiceTitanError.</returns>
            Public Shared Function getDiscountAndFees(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/discounts-and-fees/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As DiscountAndFeesResponse = JsonConvert.DeserializeObject(Of DiscountAndFeesResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of categories, depending on your parameters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/options</param>
            ''' <returns>Either returns a paginated list of categories (DiscountAndFeesResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getDiscountAndFeesList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/discounts-and-fees"

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
                    req.ContentType = "application/json"

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
                    Dim results As DiscountAndFeesResponse_P = JsonConvert.DeserializeObject(Of DiscountAndFeesResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an existing discount and/or fee
            ''' </summary>
            ''' <param name="id">The discount and/or fee's ID</param>
            ''' <param name="payload">The updated discount and/or fee</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a DiscountAndFeesResponse, or a ServiceTitanError.</returns>
            Public Shared Function updateDiscountAndFees(ByVal id As Integer, ByVal payload As DiscountAndFeesUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/discounts-and-fees/" & id

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As DiscountAndFeesResponse = JsonConvert.DeserializeObject(Of DiscountAndFeesResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function

            ''' <summary>
            ''' Creates a new equipment in the pricebook.
            ''' </summary>
            ''' <param name="payload">The new equipment to add</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either the created equipment (EquipmentResponse) or a ServiceTitanError.</returns>
            Public Shared Function createEquipment(ByVal payload As EquipmentCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/equipment"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As EquipmentResponse = JsonConvert.DeserializeObject(Of EquipmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Deletes a equipment from the pricebook using it's ID.
            ''' </summary>
            ''' <param name="id">The equipment ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>IF successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function deleteEquipment(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/equipment/" & id

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
                    req.Method = "DEL"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As EquipmentResponse = JsonConvert.DeserializeObject(Of EquipmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a equipment, using it's ID
            ''' </summary>
            ''' <param name="id">The equipment ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a EquipmentResponse, or a ServiceTitanError.</returns>
            Public Shared Function getEquipment(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/equipment/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As EquipmentResponse = JsonConvert.DeserializeObject(Of EquipmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of categories, depending on your parameters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/options</param>
            ''' <returns>Either returns a paginated list of categories (EquipmentResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getEquipmentList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/equipment"

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
                    req.ContentType = "application/json"

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
                    Dim results As EquipmentResponse_P = JsonConvert.DeserializeObject(Of EquipmentResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an existing equipment
            ''' </summary>
            ''' <param name="id">The equipment's ID</param>
            ''' <param name="payload">The updated equipment</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a EquipmentResponse, or a ServiceTitanError.</returns>
            Public Shared Function updateEquipment(ByVal id As Integer, ByVal payload As EquipmentUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/equipment/" & id

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As EquipmentResponse = JsonConvert.DeserializeObject(Of EquipmentResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function

            ''' <summary>
            ''' Creates a new material in the pricebook.
            ''' </summary>
            ''' <param name="payload">The new material to add</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either the created material (MaterialResponse) or a ServiceTitanError.</returns>
            Public Shared Function createMaterial(ByVal payload As MaterialCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/materials"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As MaterialResponse = JsonConvert.DeserializeObject(Of MaterialResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Deletes a material from the pricebook using it's ID.
            ''' </summary>
            ''' <param name="id">The material ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>IF successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function deleteMaterial(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/materials/" & id

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
                    req.Method = "DEL"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As MaterialResponse = JsonConvert.DeserializeObject(Of MaterialResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a material, using it's ID
            ''' </summary>
            ''' <param name="id">The material ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a MaterialResponse, or a ServiceTitanError.</returns>
            Public Shared Function getMaterial(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/materials/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As MaterialResponse = JsonConvert.DeserializeObject(Of MaterialResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of categories, depending on your parameters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/options</param>
            ''' <returns>Either returns a paginated list of categories (MaterialResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getMaterialList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/materials"

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
                    req.ContentType = "application/json"

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
                    Dim results As MaterialResponse_P = JsonConvert.DeserializeObject(Of MaterialResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an existing material
            ''' </summary>
            ''' <param name="id">The material's ID</param>
            ''' <param name="payload">The updated material</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a MaterialResponse, or a ServiceTitanError.</returns>
            Public Shared Function updateMaterial(ByVal id As Integer, ByVal payload As MaterialUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/materials/" & id

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As MaterialResponse = JsonConvert.DeserializeObject(Of MaterialResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function

            ''' <summary>
            ''' Creates a new service in the pricebook.
            ''' </summary>
            ''' <param name="payload">The new service to add</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either the created service (ServiceResponse) or a ServiceTitanError.</returns>
            Public Shared Function createService(ByVal payload As ServiceCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/services"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ServiceResponse = JsonConvert.DeserializeObject(Of ServiceResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Deletes a service from the pricebook using it's ID.
            ''' </summary>
            ''' <param name="id">The service ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>IF successful, returns nothing. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function deleteService(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/services/" & id

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
                    req.Method = "DEL"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As ServiceResponse = JsonConvert.DeserializeObject(Of ServiceResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a service, using it's ID
            ''' </summary>
            ''' <param name="id">The service ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a ServiceResponse, or a ServiceTitanError.</returns>
            Public Shared Function getService(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/services/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As ServiceResponse = JsonConvert.DeserializeObject(Of ServiceResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of categories, depending on your parameters.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/options</param>
            ''' <returns>Either returns a paginated list of categories (ServiceResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getServiceList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/services"

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
                    req.ContentType = "application/json"

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
                    Dim results As ServiceResponse_P = JsonConvert.DeserializeObject(Of ServiceResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an existing service
            ''' </summary>
            ''' <param name="id">The service's ID</param>
            ''' <param name="payload">The updated service</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a ServiceResponse, or a ServiceTitanError.</returns>
            Public Shared Function updateService(ByVal id As Integer, ByVal payload As ServiceUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/pricebook/v2/tenant/" & tenant & "/services/" & id

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
                    req.Method = "PATCH"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As ServiceResponse = JsonConvert.DeserializeObject(Of ServiceResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class SalesAndEstimates
            Inherits Types.SalesAndEstimates
            ''' <summary>
            ''' Creates a new estimate.
            ''' </summary>
            ''' <param name="payload">The new estimate to create</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns an EstimateResponse, or a ServiceTitanError.</returns>
            Public Shared Function createEstimate(ByVal payload As CreateEstimateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/sales/v2/tenant/" & tenant & "/estimates"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As EstimateResponse = JsonConvert.DeserializeObject(Of EstimateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Delete an estimate's item.
            ''' </summary>
            ''' <param name="estimateId">The estimate ID</param>
            ''' <param name="estimateItemId">The estimate item ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns Nothing if successful. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function deleteEstimateItem(ByVal estimateId As Integer, ByVal estimateItemId As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/sales/v2/tenant/" & tenant & "/estimates/" & estimateId & "/items/" & estimateItemId

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
                    req.Method = "DEL"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As EstimateResponse = JsonConvert.DeserializeObject(Of EstimateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Dismisses an active estimate.
            ''' </summary>
            ''' <param name="estimateId">The estimate's ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns Nothing if successful. Otherwise, returns a ServiceTitanError.</returns>
            Public Shared Function dismissEstimate(ByVal estimateId As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/sales/v2/tenant/" & tenant & "/estimates/" & estimateId

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
                    req.Method = "PUT"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As EstimateResponse = JsonConvert.DeserializeObject(Of EstimateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets an estimate by it's ID.
            ''' </summary>
            ''' <param name="estimateId">The estimate ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns an EstimateResponse, or a ServiceTitanError.</returns>
            Public Shared Function getEstimate(ByVal estimateId As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/sales/v2/tenant/" & tenant & "/estimates/" & estimateId

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
                    req.ContentType = "application/json"

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
                    Dim results As EstimateResponse = JsonConvert.DeserializeObject(Of EstimateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of estimate items for an estimate.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Either returns a paginated list of estimate items (EstimateItemResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getEstimateItemList(ByVal estimateid As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/sales/v2/tenant/" & tenant & "/estimates/" & estimateid & "/items"

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
                    req.ContentType = "application/json"

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
                    Dim results As EstimateItemResponse_P = JsonConvert.DeserializeObject(Of EstimateItemResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of estimates.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of settings/parameters</param>
            ''' <returns>Returns either a paginated list of estimates (EstimateResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getEstimateList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/sales/v2/tenant/" & tenant & "/estimates/"

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
                    req.ContentType = "application/json"

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
                    Dim results As EstimateResponse_P = JsonConvert.DeserializeObject(Of EstimateResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an estimate item on an estimate.
            ''' </summary>
            ''' <param name="estimateid">The estimate ID</param>
            ''' <param name="payload">The updated estimate item</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either an EstimateItemUpdateResponse, or a ServiceTitanError.</returns>
            Public Shared Function updateEstimateItem(ByVal estimateid As Integer, ByVal payload As EstimateItemCreateUpdateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/sales/v2/tenant/" & tenant & "/estimates/" & estimateid

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
                    req.Method = "PUT"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As EstimateItemUpdateResponse = JsonConvert.DeserializeObject(Of EstimateItemUpdateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Sets an estimate as being 'sold'.
            ''' </summary>
            ''' <param name="estimateid">The estimate ID</param>
            ''' <param name="payload">The SellRequest (Containing the soldBy ID)</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns an EstimateResponse, or a ServiceTitanError.</returns>
            Public Shared Function sellEstimate(ByVal estimateid As Integer, ByVal payload As SellRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/sales/v2/tenant/" & tenant & "/estimates/" & estimateid & "/sell"

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
                    req.Method = "PUT"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As EstimateResponse = JsonConvert.DeserializeObject(Of EstimateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Unsells an estimate.
            ''' </summary>
            ''' <param name="estimateid">The estimate ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns Nothing (If successful), or a ServiceTitanError.</returns>
            Public Shared Function unsellEstimate(ByVal estimateid As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/sales/v2/tenant/" & tenant & "/estimates/" & estimateid & "/unsell"

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
                    req.Method = "PUT"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    'Dim results As EstimateResponse = JsonConvert.DeserializeObject(Of EstimateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Updates an estimate
            ''' </summary>
            ''' <param name="estimateid">The estimate ID</param>
            ''' <param name="payload">The updated estimate payload</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either an EstimateResponse, or a ServiceTitanError.</returns>
            Public Shared Function updateEstimate(ByVal estimateid As Integer, ByVal payload As UpdateEstimateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/sales/v2/tenant/" & tenant & "/estimates/" & estimateid

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
                    req.Method = "PUT"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As EstimateResponse = JsonConvert.DeserializeObject(Of EstimateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class Settings
            Inherits Types.Settings
            ''' <summary>
            ''' Gets a single business unit by it's ID.
            ''' </summary>
            ''' <param name="id">The business unit ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a BusinessUnitResponse, or a ServiceTitanError.</returns>
            Public Shared Function getBusinessUnit(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/settings/v2/tenant/" & tenant & "/business-units/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As BusinessUnitResponse = JsonConvert.DeserializeObject(Of BusinessUnitResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of business units based on your parameters
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Either returns a paginated list of business units (BusinessUnitResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getBusinessUnitList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/settings/v2/tenant/" & tenant & "/business-units"

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
                    req.ContentType = "application/json"

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
                    Dim results As BusinessUnitResponse_P = JsonConvert.DeserializeObject(Of BusinessUnitResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single employee by it's ID.
            ''' </summary>
            ''' <param name="id">The employee ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a EmployeeResponse, or a ServiceTitanError.</returns>
            Public Shared Function getEmployee(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/settings/v2/tenant/" & tenant & "/employees/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As EmployeeResponse = JsonConvert.DeserializeObject(Of EmployeeResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of employees based on your parameters
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Either returns a paginated list of employees (EmployeeResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getEmployeeList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/settings/v2/tenant/" & tenant & "/employees"

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
                    req.ContentType = "application/json"

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
                    Dim results As EmployeeResponse_P = JsonConvert.DeserializeObject(Of EmployeeResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function

            ''' <summary>
            ''' Gets a list of tag types based on your parameters
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Either returns a paginated list of tag types (TagTypeResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getTagTypesList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/settings/v2/tenant/" & tenant & "/employees"

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
                    req.ContentType = "application/json"

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
                    Dim results As TagTypeResponse_P = JsonConvert.DeserializeObject(Of TagTypeResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single technician by it's ID.
            ''' </summary>
            ''' <param name="id">The technician ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a TechnicianResponse, or a ServiceTitanError.</returns>
            Public Shared Function getTechnician(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/settings/v2/tenant/" & tenant & "/technicians/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As TechnicianResponse = JsonConvert.DeserializeObject(Of TechnicianResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of technicians based on your parameters
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Either returns a paginated list of technicians (TechnicianResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getTechnicianList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/settings/v2/tenant/" & tenant & "/technicians"

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
                    req.ContentType = "application/json"

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
                    Dim results As TechnicianResponse_P = JsonConvert.DeserializeObject(Of TechnicianResponse_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class TaskManagement
            Inherits Types.TaskManagement
            ''' <summary>
            ''' Gets all of the task management types in one fell swoop.
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns all of the data (ClientSideDataResponse) or a ServiceTitanError.</returns>
            Public Shared Function getTaskManagementData(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/taskmanagement/v2/tenant/" & tenant & "/data"

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
                    req.ContentType = "application/json"

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
                    Dim results As ClientSideDataResponse = JsonConvert.DeserializeObject(Of ClientSideDataResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a new Task Management task
            ''' </summary>
            ''' <param name="payload">The new task to create</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a TaskCreateResponse, or a ServiceTitanError.</returns>
            Public Shared Function createTask(ByVal payload As TaskCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/taskmanagement/v2/tenant/" & tenant & "/tasks"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As TaskCreateResponse = JsonConvert.DeserializeObject(Of TaskCreateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a Task Management subtask
            ''' </summary>
            ''' <param name="id">The main task's ID</param>
            ''' <param name="payload">The subtask to create</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a SubTaskCreateResponse, or a ServiceTitanError.</returns>
            Public Shared Function createSubtask(ByVal id As Integer, ByVal payload As SubtaskCreateRequest, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/taskmanagement/v2/tenant/" & tenant & "/tasks/" & id & "/subtasks"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As SubTaskCreateResponse = JsonConvert.DeserializeObject(Of SubTaskCreateResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
        End Class
        Public Class Telecom
            Inherits Types.Telecom
            ''' <summary>
            ''' Gets a list of call reasons
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters</param>
            ''' <returns>Either returns a paginated list of call reasons (CallReasonResponse_P) or a ServiceTitanError.</returns>
            Public Shared Function getCallReasonList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/telecom/v2/tenant/" & tenant & "/call-reasons"

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
                    req.ContentType = "application/json"

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
                    Dim results As CallReasonResponse = JsonConvert.DeserializeObject(Of CallReasonResponse)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Creates a new call record. (NOTE: Does not actually initiate a call)
            ''' </summary>
            ''' <param name="payload">The new call record</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a DetailedCallModel, or a ServiceTitanError.</returns>
            Public Shared Function createCall(ByVal payload As CallInModel, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/telecom/v2/tenant/" & tenant & "/call-reasons"

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
                    req.Method = "POST"
                    req.Timeout = 999999
                    req.Headers.Add("ST-App-Key", STAppKey)
                    req.Headers.Add("Authorization", accesstoken.access_token)
                    req.ContentType = "application/json"

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
                    Dim results As DetailedCallModel = JsonConvert.DeserializeObject(Of DetailedCallModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a list of calls
            ''' </summary>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <param name="options">A list of parameters/filters.</param>
            ''' <returns>Either returns a paginated list of calls (BundleCallModel_P) or a ServiceTitanError.</returns>
            Public Shared Function getCallList(ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer, Optional ByVal options As List(Of OptionsList) = Nothing) As Object

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
                    Dim baseurl As String = domain & "/telecom/v2/tenant/" & tenant & "/calls"

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
                    req.ContentType = "application/json"

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
                    Dim results As BundleCallModel_P = JsonConvert.DeserializeObject(Of BundleCallModel_P)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a single call by it's ID.
            ''' </summary>
            ''' <param name="id">The Call ID</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Either returns a DetailedBundleCallModel, or a ServiceTitanError.</returns>
            Public Shared Function getCall(ByVal id As Integer, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/telecom/v2/tenant/" & tenant & "/calls/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As DetailedBundleCallModel = JsonConvert.DeserializeObject(Of DetailedBundleCallModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
            ''' <summary>
            ''' Gets a call recording, and saves it on disk. (mp3)
            ''' </summary>
            ''' <param name="id">The call ID</param>
            ''' <param name="saveLocation">The full path on disk where to save the file.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>IF the file is not found, a webclient exception will be thrown. If successful, the file will be saved on disk.</returns>
            Public Shared Function getCallRecording(ByVal id As Integer, ByVal saveLocation As String, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)


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
                Dim baseurl As String = domain & "/telecom/v2/tenant/" & tenant & "/calls/" & id & "/recording"

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
                Dim client As New WebClient
                client.Headers.Add("ST-App-Key", STAppKey)
                client.Headers.Add("Authorization", accesstoken.access_token)
                client.DownloadFile(baseurl, saveLocation)


                'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                'req.ContentLength = bytearray.Length
                'req.Timeout = 999999
                'Dim datastream As Stream = req.GetRequestStream
                'datastream.Write(bytearray, 0, bytearray.Length)
                'datastream.Close()



            End Function
            ''' <summary>
            ''' Gets a call voicemail, and saves it on disk. (mp3)
            ''' </summary>
            ''' <param name="id">The call ID</param>
            ''' <param name="saveLocation">The full path on disk where to save the file.</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>IF the file is not found, a webclient exception will be thrown. If successful, the file will be saved on disk.</returns>
            Public Shared Function getCallVoicemail(ByVal id As Integer, ByVal saveLocation As String, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer)


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
                Dim baseurl As String = domain & "/telecom/v2/tenant/" & tenant & "/calls/" & id & "/voicemail"

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
                Dim client As New WebClient
                client.Headers.Add("ST-App-Key", STAppKey)
                client.Headers.Add("Authorization", accesstoken.access_token)
                client.DownloadFile(baseurl, saveLocation)


                'Dim bytearray() As Byte = Encoding.UTF8.GetBytes(JsonConvert.SerializeObject(payload))
                'req.ContentLength = bytearray.Length
                'req.Timeout = 999999
                'Dim datastream As Stream = req.GetRequestStream
                'datastream.Write(bytearray, 0, bytearray.Length)
                'datastream.Close()



            End Function
            ''' <summary>
            ''' Updates a call record.
            ''' </summary>
            ''' <param name="id">The Call ID</param>
            ''' <param name="payload">The updated call payload</param>
            ''' <param name="accesstoken">The full oAuth2.AccessToken class containing your credentials.</param>
            ''' <param name="STAppKey">Your ServiceTitan Application Key</param>
            ''' <param name="tenant">Your Tenant ID</param>
            ''' <returns>Returns either a DetailedCallModel, or a ServiceTitanError.</returns>
            Public Shared Function updateCall(ByVal id As Integer, ByVal payload As CallInUpdateModel, ByVal accesstoken As oAuth2.AccessToken, ByVal STAppKey As String, ByVal tenant As Integer) As Object

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
                    Dim baseurl As String = domain & "/telecom/v2/tenant/" & tenant & "/calls/" & id

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
                    req.ContentType = "application/json"

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
                    Dim results As DetailedCallModel = JsonConvert.DeserializeObject(Of DetailedCallModel)(output, New JsonSerializerSettings With {.NullValueHandling = NullValueHandling.Ignore})
                    'Dim results As Object = Nothing
                    streamread.Close()
                    buffer.Close()
                    response.Close()
                    Return results
                Catch ex As WebException
                    Dim newerror As ServiceTitanError = ErrorHandling.ProcessError(ex)
                    Return newerror
                End Try
            End Function
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
