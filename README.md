# TCC-Digital-Bones
App de realidade aumentada para ensino de anatomia óssea veterinária, com modelos 3D gerados a partir de tomografias (TC/RM). Squad Digital Bones · UNIFACS

# Digital Bones AR

> Aplicativo de realidade aumentada para o ensino de anatomia óssea veterinária, construído a partir de modelos 3D gerados de exames de tomografia computadorizada.

Projeto do **Squad Digital Bones** · Universidade Salvador (UNIFACS)
Orientador: Euclerio Ornellas · Salvador – BA, 2026

---

## Sobre o projeto

O Digital Bones integra **Tecnologia** e **Medicina Veterinária** para transformar exames estáticos de imagem em ativos educacionais interativos. A partir de dados de TC, geramos modelos 3D de ossos de animais que podem ser:

- explorados em **realidade aumentada**, com percepção de profundidade que telas comuns não oferecem;
- impressos em 3D como **réplicas físicas táteis e de baixo custo**;
- usados em aulas práticas, "dissecação digital" e, futuramente, planejamento cirúrgico.

O objetivo é levar o aprendizado teórico-prático para uma experiência digital e tátil de alta precisão, preparando futuros profissionais para uma atuação clínica e cirúrgica mais assertiva.

## 🔄 Pipeline do projeto

| Etapa | Descrição |
|---|---|
| 1. Aquisição de dados | Imagens volumétricas de Tomografia Computadorizada (TC) |
| 2. Segmentação | Processamento dos arquivos DICOM em software de modelagem 3D para isolar as estruturas ósseas |
| 3. Refinamento 3D | Conversão em malha poligonal (STL), com correção de falhas e garantia de precisão anatômica |
| 4. Experiência imersiva | Importação do modelo para AR/VR, com interação por gestos e hand tracking |
| 5. Impressão 3D | Réplicas físicas, com densidade de preenchimento (infill) validada em testes com cirurgião veterinário |

## Funcionalidades

Roadmap de funcionalidades do app (priorizadas por valor × facilidade):

**MVP**
- [ ] Visualizador AR de modelo ósseo 3D
- [ ] Interação por gestos (girar, mover, escalar)
- [ ] Pipeline de modelos: STL → formato otimizado para AR
- [ ] Mostrar/ocultar e isolar estruturas (dissecação digital básica)
- [ ] Rótulos anatômicos interativos
- [ ] Catálogo de modelos (ossos e espécies)
- [ ] Modo escala real (1:1)
- [ ] Teste de usabilidade com alunos e cirurgião veterinário

**Fase 2**
- [ ] AR sobre a réplica impressa (marcador/QR code)
- [ ] Modo quiz de identificação de estruturas
- [ ] Busca, filtros e favoritos
- [ ] Vista explodida e corte/seção transversal
- [ ] Suporte a headset com hand tracking
- [ ] Modo offline

** Fase 3**
- [ ] Simulador de planejamento cirúrgico
- [ ] Modo aula multiusuário (professor + alunos)
- [ ] Painel do professor e importação de DICOM
- [ ] Analytics de aprendizagem e acessibilidade

##  Testes de densidade de impressão 3D

Para que as réplicas se comportem de forma próxima ao osso real, testamos diferentes percentuais de preenchimento (infill). Com acompanhamento de um cirurgião veterinário, simulamos a inserção de parafuso com chave de fenda em cada amostra, avaliando resistência, textura e comportamento mecânico. Os resultados definem o padrão de impressão das próximas réplicas, equilibrando fidelidade anatômica, desempenho mecânico e custo.

## Tecnologias

> Ajuste conforme o stack final do projeto.

- **Imagem médica:** DICOM, TC, STL, GLB
- **Modelagem 3D:** Blender
- **App AR:** UNITY
- **Dispositivos:** Meta Quest 3
- **Fabricação:** impressão 3D FDM

## Como executar

```bash
# Clone o repositório
git clone https://github.com/<usuario>/<repositorio>.git
cd <repositorio>

# Instruções de build serão adicionadas conforme o MVP avança
```

##Estrutura do repositório

```
├── app/            # Código do aplicativo AR
├── models/         # Modelos 3D (STL / GLB)
├── docs/           # Relatórios e documentação
├── print-tests/    # Resultados dos testes de densidade
└── README.md
```

## Status

 **Em desenvolvimento.** O projeto iniciou em 2025/1 e está na fase de testes do protótipo de realidade aumentada e de validação das réplicas impressas.

## Equipe

**Squad Digital Bones** · UNIFACS


<!-- Adicione aqui os nomes dos integrantes -->


