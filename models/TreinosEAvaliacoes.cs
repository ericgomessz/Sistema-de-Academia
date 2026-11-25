using System;
using System.Collections.Generic;
using System.Globalization;

namespace NexusFit.Models
{
    public abstract class Exercicio
    {
        private readonly string _nome;

        protected Exercicio(string nome)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new RegraNegocioException("Nome do exercício é obrigatório.");
            _nome = nome.Trim();
        }

        public string Nome => _nome;
        public abstract string Descrever();
    }

    public class Aerobico : Exercicio
    {
        public Aerobico(string nome, int duracaoMinutos, string intensidade) : base(nome)
        {
            if (duracaoMinutos <= 0)
                throw new RegraNegocioException("Duração deve ser maior que zero minutos.");
            if (string.IsNullOrWhiteSpace(intensidade))
                throw new RegraNegocioException("Intensidade é obrigatória.");

            DuracaoMinutos = duracaoMinutos;
            Intensidade = intensidade.Trim();
        }

        public int DuracaoMinutos { get; }
        public string Intensidade { get; }

        public override string Descrever() =>
            $"[Aeróbico] {Nome}: {DuracaoMinutos} min, intensidade {Intensidade}";
    }

    public class Musculacao : Exercicio
    {
        public Musculacao(string nome, int series, int repeticoes, decimal carga) : base(nome)
        {
            if (series <= 0 || repeticoes <= 0)
                throw new RegraNegocioException("Séries e repetições devem ser maiores que zero.");
            if (carga < 0)
                throw new RegraNegocioException("Carga não pode ser negativa.");

            Series = series;
            Repeticoes = repeticoes;
            Carga = carga;
        }

        public int Series { get; }
        public int Repeticoes { get; }
        public decimal Carga { get; }

        public override string Descrever() =>
            $"[Musculação] {Nome}: {Series}x{Repeticoes}, {Carga.ToString("0.#", CultureInfo.InvariantCulture)} kg";
    }

    public class Treino
    {
        private readonly List<Exercicio> _exercicios = new();

        public Treino(string nome, Instrutor instrutorResponsavel)
        {
            if (string.IsNullOrWhiteSpace(nome))
                throw new RegraNegocioException("Nome do treino é obrigatório.");
            if (instrutorResponsavel == null)
                throw new RegraNegocioException("Treino deve ter um instrutor responsável.");

            Nome = nome.Trim();
            InstrutorResponsavel = instrutorResponsavel;
            DataCriacao = DateTime.Today;
        }

        public string Nome { get; }
        public DateTime DataCriacao { get; }
        public Instrutor InstrutorResponsavel { get; }
        public IReadOnlyList<Exercicio> Exercicios => _exercicios.AsReadOnly();

        public void AdicionarExercicio(Exercicio exercicio)
        {
            if (exercicio == null)
                throw new RegraNegocioException("Exercício não pode ser nulo.");
            _exercicios.Add(exercicio);
        }
    }

    public sealed class MedidasCorporais
    {
        public MedidasCorporais(decimal cinturaCm, decimal quadrilCm, decimal toraxCm, decimal bracoCm)
        {
            if (cinturaCm <= 0 || quadrilCm <= 0 || toraxCm <= 0 || bracoCm <= 0)
                throw new RegraNegocioException("Todas as medidas corporais devem ser maiores que zero.");

            CinturaCm = cinturaCm;
            QuadrilCm = quadrilCm;
            ToraxCm = toraxCm;
            BracoCm = bracoCm;
        }

        public decimal CinturaCm { get; }
        public decimal QuadrilCm { get; }
        public decimal ToraxCm { get; }
        public decimal BracoCm { get; }

        public override string ToString() =>
            $"Cintura: {CinturaCm:F1}cm | Quadril: {QuadrilCm:F1}cm | Tórax: {ToraxCm:F1}cm | Braço: {BracoCm:F1}cm";
    }

    public class AvaliacaoFisica
    {
        public AvaliacaoFisica(DateTime data, decimal peso, MedidasCorporais medidas, Instrutor instrutor)
        {
            if (peso <= 0)
                throw new RegraNegocioException("Peso deve ser maior que zero.");
            if (medidas == null)
                throw new RegraNegocioException("Medidas corporais são obrigatórias.");
            if (instrutor == null)
                throw new RegraNegocioException("Instrutor é obrigatório.");

            Data = data;
            Peso = peso;
            Medidas = medidas;
            Instrutor = instrutor;
        }

        public DateTime Data { get; }
        public decimal Peso { get; }
        public MedidasCorporais Medidas { get; }
        public Instrutor Instrutor { get; }
    }
}
