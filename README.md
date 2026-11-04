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

Mantenha os dados identificadores dos exames fora dos assets públicos do aplicativo.

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

**Fase 3**
- [ ] Simulador de planejamento cirúrgico
- [ ] Modo aula multiusuário (professor + alunos)
- [ ] Painel do professor e importação de DICOM
- [ ] Analytics de aprendizagem e acessibilidade

## Testes de densidade de impressão 3D

Para que as réplicas se comportem de forma próxima ao osso real, testamos diferentes percentuais de preenchimento (infill). Com acompanhamento de um cirurgião veterinário, simulamos a inserção de parafuso com chave de fenda em cada amostra, avaliando resistência, textura e comportamento mecânico. Os resultados definem o padrão de impressão das próximas réplicas, equilibrando fidelidade anatômica, desempenho mecânico e custo.

## Tecnologias

> Ajuste conforme o stack final do projeto.

- **Imagem médica:** DICOM, TC, STL, GLB
- **Modelagem 3D:** Blender
- **App AR:** Unity
- **Dispositivos:** Meta Quest 3
- **Fabricação:** impressão 3D FDM

## Como executar

1. Clone este repositório.
2. Abra a pasta no Unity `6000.3.27f2`.
3. Abra a cena `Assets/Scenes/AnatomyLab.unity`.

O projeto usa OpenXR. O loader Android, a integração Meta XR e os perfis de controle ainda precisam ser configurados e validados para o dispositivo alvo. OpenXR, por si só, não habilita passthrough no Quest.

## Estrutura do repositório

```text
Assets/          Cenas, scripts, materiais e configurações XR
Packages/        Dependências do Unity
ProjectSettings/ Configurações do projeto
```

## Protótipo Unity

A cena de demonstração usa primitivas para representar um fêmur simplificado. Elas são placeholders didáticos, não modelos anatômicos de precisão.

No desktop, clique para selecionar, arraste com o botão direito para girar e use a roda do mouse para escalar. `Space` ativa a separação, `I` isola a estrutura selecionada, `R` restaura e `1` a `3` selecionam as três regiões.

Com XR ativo, a câmera acompanha a pose do headset. O gatilho direito seleciona uma estrutura, o botão A alterna a separação, o botão X isola a seleção e o analógico esquerdo gira o modelo.

`AnatomyModelDefinition` registra espécie, osso, prefab, dimensões físicas em milímetros, identificador do estudo de origem e status de validação. Crie esses registros pelo menu `Assets > Create > Digital Bones > Anatomy Model`. `AnatomicalPart` representa uma estrutura selecionável, e `AnatomyViewer` coordena a experiência atual e recebe entradas de desktop e controles XR.

O editor usado é Unity `6000.3.27f2`, com Android Build Support, Android SDK/NDK e OpenJDK.

## Status

**Em desenvolvimento.** O projeto iniciou em 2025/1 e está na fase de testes do protótipo de realidade aumentada e de validação das réplicas impressas.

## Equipe

**Squad Digital Bones** · UNIFACS

<!-- Adicione aqui os nomes dos integrantes -->
