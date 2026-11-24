# 1. Apresentação do Sistema

## 1.1. O que o sistema faz?

O NexusFit automatiza e centraliza a gestão operacional de uma academia. Ele cobre o ciclo de vida do membro: cadastro, contratação e renovação de plano, controle de pagamentos e status de matrícula, montagem de treinos, registro de avaliações físicas e acompanhamento da evolução.

## 1.2. Para quem serve?

O sistema possui um **módulo de autenticação unificado** que atende três perfis:

| Perfil | O que pode fazer |
|---|---|
| 👤 **Membro (Aluno)** | Fazer login, contratar/renovar/alterar plano, solicitar cancelamento ou trancamento (quando o plano permitir), consultar pagamentos e status da matrícula, visualizar seus treinos e consultar sua evolução física. |
| 👨‍💼 **Funcionário** | Cadastrar membros, consultar dados cadastrais e pendências (ativo/inativo), efetuar cobranças e cancelar matrículas. |
| 🧑‍🏫 **Instrutor** (especialização de Funcionário) | Tudo o que o Funcionário faz, além de montar treinos e registrar avaliações físicas. |

## 1.3. Principais Funcionalidades

- 🔐 **Autenticação e cadastro:** login unificado que direciona o usuário ao painel do seu perfil; cadastro de membros e instrutores.
- 📆 **Planos por duração (Mensal, Trimestral, Anual):** cada plano calcula sua mensalidade e define se permite trancamento.
- 🥇 **Categorias de benefício (Prata, Ouro, Diamante):** definem dias de acesso, aulas e benefícios extras.
- 💳 **Controle financeiro:** registro de pagamentos, pendências e status de atividade do membro.
- 📋 **Treinos:** montados por um instrutor, compostos por exercícios aeróbicos e de musculação.
- 📏 **Avaliações físicas:** registro de data, peso e medidas, com consulta da evolução ao longo do tempo.

---

# 2. Regras de Negócio

| Código | Regra |
|---|---|
| RN01 | Cada membro possui **um único plano ativo por vez**. |
| RN02 | A **mensalidade varia com o plano** (duração + categoria) **e com o tempo de fidelidade** do membro. |
| RN03 | O **treino é montado por um instrutor**. |
| RN04 | A **avaliação física registra data, peso e medidas**. |
| RN05 | O acesso do membro é liberado ou restringido conforme seu **status** (ativo/inativo) e suas pendências financeiras. |
| RN06 | Apenas planos que **permitem trancamento** aceitam essa solicitação. |

## 2.1. Categorias de benefício

| Categoria | Benefícios |
|---|---|
| 💎 **Diamante** | Acesso 7 dias por semana, aulas de bicicleta, aulas de box e direito a levar 3 amigos por mês. |
| 🥇 **Ouro** | Acesso 7 dias por semana, aulas de bicicleta e aulas de box. |
| 🥈 **Prata (Básico)** | Acesso 5 dias por semana e 1 aula por mês de livre escolha. |

## 2.2. Planos por duração (proposta de regra, ajustável)

| Plano | Desconto sobre o valor base | Permite trancamento? |
|---|---|---|
| **Mensal** | Nenhum | Não |
| **Trimestral** | Desconto de contratação trimestral | Sim |
| **Anual** | Maior desconto de contratação | Sim |

> Os percentuais exatos (desconto por duração e por fidelidade) são parâmetros do projeto, definidos em um único ponto de configuração no código.

**Cálculo da mensalidade:**

```
mensalidade = valorBase(categoria) × (1 − descontoDuração) × (1 − descontoFidelidade(mesesDeFidelidade))
```
