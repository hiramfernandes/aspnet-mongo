using System.Text.Json.Serialization;

namespace Purchases.Application.Models
{
    public class NfcReceipt
    {
        [JsonPropertyName("document_type")]
        public string? DocumentType { get; set; }

        [JsonPropertyName("source")]
        public DataSource? Source { get; set; }

        public QR? QR { get; set; }

        [JsonPropertyName("merchant")]
        public Merchant? Merchant { get; set; }

        [JsonPropertyName("transaction")]
        public Transaction? Transaction { get; set; }

        [JsonPropertyName("totals")]
        public Totals? Totals { get; set; }

        [JsonPropertyName("taxes")]
        public Taxes? Taxes { get; set; }

        [JsonPropertyName("items")]
        public List<Item>? Items { get; set; }

        [JsonPropertyName("retrieval")]
        public Retrieval? Retrieval { get; set; }
    }

    public class DataSource
    {
        [JsonPropertyName("source_type")]
        public string? SourceType { get; set; }

        [JsonPropertyName("image_file")]
        public string? ImageFile { get; set; }

        [JsonPropertyName("ingested_at")]
        public DateTime? IngestedAt { get; set; }

        [JsonPropertyName("ocr_confidence_overall")]
        public float? OcrConfidencOverall { get; set; }

        [JsonPropertyName("notes")]
        public string[]? Notes { get; set; }
    }
    public class QR
    {
        [JsonPropertyName("decoded_ok")]
        public bool? DecodedOk { get; set; }

        [JsonPropertyName("payload_raw")]
        public string? PayloadRaw { get; set; }

        [JsonPropertyName("payload_type")]
        public string? PayloadType { get; set; }

        [JsonPropertyName("url")]
        public string? Url { get; set; }

        [JsonPropertyName("fallback_used")]
        public bool? FallbackUsed { get; set; }

        [JsonPropertyName("fallback_source")]
        public string? FallbackSource { get; set; }
    }

    public class Merchant
    {
        [JsonPropertyName("legal_name")]
        public string? LegalName { get; set; }

        [JsonPropertyName("trade_name")]
        public string? TradeName { get; set; }

        [JsonPropertyName("cnpj")]
        public string? Cnpj { get; set; }

        [JsonPropertyName("ie")]
        public string? Ie { get; set; }

        [JsonPropertyName("im")]
        public string? Im { get; set; }

        [JsonPropertyName("address")]
        public Address? Address { get; set; }
    }

    public class Address
    {
        [JsonPropertyName("street")]
        public string? Street { get; set; }

        [JsonPropertyName("number")]
        public string? Number { get; set; }

        [JsonPropertyName("neighborhood")]
        public string? Neighborhood { get; set; }

        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }

        [JsonPropertyName("zip")]
        public string? Zip { get; set; }

        [JsonPropertyName("country")]
        public string? Country { get; set; }
    }

    public class Transaction
    {
        [JsonPropertyName("model")]
        public string? Model { get; set; }

        [JsonPropertyName("nfce_number")]
        public string? NfceNumber { get; set; }

        [JsonPropertyName("series")]
        public string? Series { get; set; }

        [JsonPropertyName("access_key_44")]
        public string? AccessKey44 { get; set; }

        [JsonPropertyName("access_key_decomposed")]
        public AccessKeyDecomposed? AccessKeyDecomposed { get; set; }

        [JsonPropertyName("issue_datetime")]
        public string? IssueDatetime { get; set; }

        [JsonPropertyName("authorization_protocol")]
        public string? AuthorizationProtocol { get; set; }

        [JsonPropertyName("authorization_datetime")]
        public DateTime? AuthorizationDatetime { get; set; }

        [JsonPropertyName("environment")]
        public string? Environment { get; set; }

        [JsonPropertyName("xml_version")]
        public string? XmlVersion { get; set; }

        [JsonPropertyName("xslt_version")]
        public string? XsltVersion { get; set; }

        [JsonPropertyName("terminal")]
        public string? Terminal { get; set; }

        [JsonPropertyName("pos_id")]
        public string? PosId { get; set; }

        [JsonPropertyName("operator")]
        public string? Operator { get; set; }

        [JsonPropertyName("consumer_identified")]
        public bool? ConsumerIdentified { get; set; }
    }

    public class AccessKeyDecomposed
    {
        [JsonPropertyName("cUF")]
        public string? CUF { get; set; }

        [JsonPropertyName("AAMM")]
        public string? AAMM { get; set; }

        [JsonPropertyName("cnpj_digits")]
        public string? CnpjDigits { get; set; }

        [JsonPropertyName("mod")]
        public string? Mod { get; set; }

        [JsonPropertyName("serie")]
        public string? Serie { get; set; }

        [JsonPropertyName("nNF")]
        public string? NNF { get; set; }
    }

    public class Totals
    {
        [JsonPropertyName("items_count")]
        public int? ItemsCount { get; set; }

        [JsonPropertyName("subtotal")]
        public double? Subtotal { get; set; }

        [JsonPropertyName("discount")]
        public object? Discount { get; set; }

        [JsonPropertyName("surcharge")]
        public object? Surcharge { get; set; }

        [JsonPropertyName("total")]
        public double? Total { get; set; }

        [JsonPropertyName("currency")]
        public string? Currency { get; set; }

        [JsonPropertyName("payment")]
        public List<Payment>? Payment { get; set; }
    }

    public class Payment
    {
        [JsonPropertyName("method")]
        public string? Method { get; set; }

        [JsonPropertyName("amount")]
        public double? Amount { get; set; }

        [JsonPropertyName("card_brand")]
        public string? CardBrand { get; set; }

        [JsonPropertyName("auth_code")]
        public object? AuthCode { get; set; }
    }

    public class Taxes
    {
        [JsonPropertyName("taxes_total_incidents_displayed")]
        public double? TaxesTotalIncidentsDisplayed { get; set; }

        [JsonPropertyName("ibpt_approx")]
        public IbptApprox? IbptApprox { get; set; }
    }

    public class IbptApprox
    {
        [JsonPropertyName("federal")]
        public double? Federal { get; set; }

        [JsonPropertyName("state")]
        public double? State { get; set; }

        [JsonPropertyName("source")]
        public string? Source { get; set; }

        [JsonPropertyName("source_code")]
        public string? SourceCode { get; set; }
    }

    public class Item
    {
        [JsonPropertyName("line_number")]
        public int? LineNumber { get; set; }

        [JsonPropertyName("description_raw")]
        public string? DescriptionRaw { get; set; }

        [JsonPropertyName("product_code_raw")]
        public string? ProductCodeRaw { get; set; }

        [JsonPropertyName("quantity")]
        public double? Quantity { get; set; }

        [JsonPropertyName("unit")]
        public string? Unit { get; set; }

        [JsonPropertyName("unit_price")]
        public double? UnitPrice { get; set; }

        [JsonPropertyName("total_price")]
        public double? TotalPrice { get; set; }

        [JsonPropertyName("discount")]
        public string? Discount { get; set; }

        [JsonPropertyName("tags")]
        public List<string>? Tags { get; set; }

        [JsonPropertyName("confidence")]
        public Confidence? Confidence { get; set; }
    }

    public class Confidence
    {
        [JsonPropertyName("description")]
        public double? Description { get; set; }

        [JsonPropertyName("quantity")]
        public double? Quantity { get; set; }

        [JsonPropertyName("prices")]
        public double? Prices { get; set; }
    }

    public class Retrieval
    {
        [JsonPropertyName("fingerprints")]
        public Fingerprints? Fingerprints { get; set; }

        [JsonPropertyName("keywords")]
        public List<string>? Keywords { get; set; }

        [JsonPropertyName("geo_hint")]
        public GeoHint? GeoHint { get; set; }
    }

    public class Fingerprints
    {
        [JsonPropertyName("merchant_cnpj_digits")]
        public string? MerchantCnpjDigits { get; set; }

        [JsonPropertyName("issue_date")]
        public string? IssueDate { get; set; }

        [JsonPropertyName("total_brl_cents")]
        public int? TotalBrlCents { get; set; }
    }

    public class GeoHint
    {
        [JsonPropertyName("city")]
        public string? City { get; set; }

        [JsonPropertyName("state")]
        public string? State { get; set; }
    }

}
