namespace apa.BOL.CosmosDB
{
    //--------------------------------------------------------------------------------------------------------------
    public class PurchaseFoodOrBeverage : IInteraction
    {
        public string id { get; set; }
        public decimal unitPrice { get; set; }
        public decimal totalPrice { get; set; }
        public int quantity { get; set; }
        public string type { get; set; }
    }
}