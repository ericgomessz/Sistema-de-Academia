using System;

namespace NexusFit.Models
{
    public abstract class Plano
    {
        protected Plano(CategoriaPlano categoria, int duracaoMeses, string descricaoDuracao)
        {
            if (duracaoMeses <= 0)
                throw new RegraNegocioException("Duração do plano deve ser maior que zero.");

            Categoria = categoria;
            DuracaoMeses = duracaoMeses;
            DescricaoDuracao = descricaoDuracao;
        }

        public CategoriaPlano Categoria { get; }
        public int DuracaoMeses { get; }
        public string DescricaoDuracao { get; }

        public abstract decimal CalcularMensalidade(int mesesFidelidade);
        public abstract bool PermiteTrancamento();
    }

    public class PlanoMensal : Plano
    {
        public PlanoMensal(CategoriaPlano categoria) : base(categoria, 1, "Mensal") { }

        public override decimal CalcularMensalidade(int mesesFidelidade)
        {
            decimal baseValor = RegrasNegocioConfig.ObterValorBaseCategoria(Categoria);
            decimal descFidelidade = RegrasNegocioConfig.CalcularDescontoFidelidade(mesesFidelidade);
            return Math.Round(baseValor * (1m - descFidelidade), 2);
        }

        public override bool PermiteTrancamento() => false;
    }

    public class PlanoTrimestral : Plano
    {
        public PlanoTrimestral(CategoriaPlano categoria) : base(categoria, 3, "Trimestral") { }

        public override decimal CalcularMensalidade(int mesesFidelidade)
        {
            decimal baseValor = RegrasNegocioConfig.ObterValorBaseCategoria(Categoria);
            decimal descFidelidade = RegrasNegocioConfig.CalcularDescontoFidelidade(mesesFidelidade);
            return Math.Round(baseValor * (1m - RegrasNegocioConfig.DescontoTrimestral) * (1m - descFidelidade), 2);
        }

        public override bool PermiteTrancamento() => true;
    }

    public class PlanoAnual : Plano
    {
        public PlanoAnual(CategoriaPlano categoria) : base(categoria, 12, "Anual") { }

        public override decimal CalcularMensalidade(int mesesFidelidade)
        {
            decimal baseValor = RegrasNegocioConfig.ObterValorBaseCategoria(Categoria);
            decimal descFidelidade = RegrasNegocioConfig.CalcularDescontoFidelidade(mesesFidelidade);
            return Math.Round(baseValor * (1m - RegrasNegocioConfig.DescontoAnual) * (1m - descFidelidade), 2);
        }

        public override bool PermiteTrancamento() => true;
    }
}
