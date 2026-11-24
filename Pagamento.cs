using System;

namespace NexusFit.Models
{
    public class Pagamento
    {
        public Pagamento(decimal valor, DateTime vencimento)
        {
            if (valor <= 0)
                throw new RegraNegocioException("Valor do pagamento deve ser maior que zero.");

            Valor = valor;
            Vencimento = vencimento;
            Pago = false;
        }

        public decimal Valor { get; }
        public DateTime Vencimento { get; }
        public bool Pago { get; private set; }

        public void MarcarComoPago() => Pago = true;
    }
}
