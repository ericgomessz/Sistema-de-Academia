using System;

namespace NexusFit.Models
{
    public enum CategoriaPlano
    {
        Prata = 1,
        Ouro = 2,
        Diamante = 3
    }

    public enum StatusMatricula
    {
        Ativa,
        Inativa,
        Trancada,
        Cancelada
    }

    public class RegraNegocioException : Exception
    {
        public RegraNegocioException(string message) : base(message) { }
    }

    public class AutenticacaoException : Exception
    {
        public AutenticacaoException(string message) : base(message) { }
    }

    public static class RegrasNegocioConfig
    {
        public const decimal DescontoTrimestral = 0.10m; // 10%
        public const decimal DescontoAnual = 0.20m;      // 20%
        public const decimal DescontoFidelidadePorAno = 0.03m;
        public const decimal TetoDescontoFidelidade = 0.15m;
        public const int LimiteConvitesDiamanteMes = 3;
        public const int LimiteAulaLivrePrataMes = 1;

        public static decimal ObterValorBaseCategoria(CategoriaPlano categoria) => categoria switch
        {
            CategoriaPlano.Prata => 110.00m,
            CategoriaPlano.Ouro => 150.00m,
            CategoriaPlano.Diamante => 200.00m,
            _ => throw new ArgumentOutOfRangeException(nameof(categoria), "Categoria de plano inválida.")
        };

        public static decimal CalcularDescontoFidelidade(int mesesFidelidade)
        {
            if (mesesFidelidade < 0)
                throw new RegraNegocioException("Meses de fidelidade não podem ser negativos.");

            int anosCompletos = mesesFidelidade / 12;
            return Math.Min(anosCompletos * DescontoFidelidadePorAno, TetoDescontoFidelidade);
        }

        public static string DescreverBeneficios(CategoriaPlano categoria) => categoria switch
        {
            CategoriaPlano.Diamante => "7 dias/semana, Bike, Box e até 3 convites de amigos/mês",
            CategoriaPlano.Ouro => "7 dias/semana, Bike e Box",
            CategoriaPlano.Prata => "5 dias/semana e 1 aula avulsa por mês",
            _ => "Categoria não definida"
        };
    }
}
