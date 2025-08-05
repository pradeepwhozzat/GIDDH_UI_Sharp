using System.Text.Json.Serialization;
using System.Runtime.Serialization;

namespace GiddhTemplate.Models.Enums
{
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public enum VoucherTypeEnums
    {
        [EnumMember(Value = "Sales")]
        Sales,

        [EnumMember(Value = "Purchase")]
        Purchase,

        [EnumMember(Value = "Purchase Order")]
        PurchaseOrder,

        [EnumMember(Value = "Purchase Bill")]
        PurchaseBill,

        [EnumMember(Value = "Debit Note")]
        DebitNote,

        [EnumMember(Value = "Credit Note")]
        CreditNote,

        [EnumMember(Value = "Proforma")]
        Proforma,

        [EnumMember(Value = "Proformas")]
        GenerateProforma,

        [EnumMember(Value = "Estimate")]
        Estimate,

        [EnumMember(Value = "Estimates")]
        GenerateEstimate,

        [EnumMember(Value = "Cash")]
        Cash,

        [EnumMember(Value = "Receipt")]
        Receipt,

        [EnumMember(Value = "Payment")]
        Payment,

        [EnumMember(Value = "Cash Debit Note")]
        CashDebitNote,

        [EnumMember(Value = "Cash Credit Note")]
        CashCreditNote,

        [EnumMember(Value = "Cash Bill")]
        CashBill
    }

    public static class CommonEnumsExtensions
    {
        public static string GetVoucherTypeEnumValue(this VoucherTypeEnums value)
        {
            var field = value.GetType().GetField(value.ToString());
            var attribute = (EnumMemberAttribute)Attribute.GetCustomAttribute(field, typeof(EnumMemberAttribute));
            return attribute?.Value ?? value.ToString();
        }
    }
}
