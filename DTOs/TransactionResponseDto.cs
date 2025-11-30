namespace TrabajoPractico.DTOs
{
    public class TransactionResponseDto
    {
            public int Id { get; set; }
            public string Action { get; set; }       
            public string Crypto_Code { get; set; }      
            public decimal Crypto_Amount { get; set; }   
            public string Money { get; set; }             
            public string Datetime { get; set; }         
    }
}
