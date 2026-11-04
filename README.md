# Digital Bones

Protótipo Unity para transformar exames de TC em modelos anatômicos 3D usados em aulas de anatomia veterinária. O projeto organiza o domínio dos modelos separadamente da experiência de visualização, para permitir evoluir do placeholder atual para um catálogo de malhas validadas.

## Organização

```text
Assets/
  Editor/                     Ferramentas de criação de cenas no Editor
  Scripts/Runtime/Anatomy/    Identidade das estruturas e metadados do modelo
  Scripts/Runtime/Experience/ Seleção, interação, visualização e modos didáticos
  Scenes/                     Cenas do aplicativo
  Models/                     Prefabs e malhas anatômicas importadas (a criar)
```

`AnatomyModelDefinition` é o registro de catálogo por modelo: espécie, osso, prefab, dimensões físicas em milímetros, identificador do estudo de origem e status de validação. Crie esses registros pelo menu `Assets > Create > Digital Bones > Anatomy Model`. `AnatomicalPart` representa uma estrutura selecionável, com identificador, rótulo, descrição, cor e deslocamento de dissecação. `AnatomyViewer` coordena a experiência atual e recebe entradas de desktop e controles XR.

## Pipeline de conteúdo

1. Adquirir o estudo de TC e manter seus dados identificadores fora dos assets públicos do aplicativo.
2. Segmentar as estruturas em ferramenta apropriada e revisar a anatomia com profissional veterinário.
3. Corrigir a malha e exportar para um formato de intercâmbio; importar e otimizar no Unity.
4. Criar um prefab por modelo, com um `AnatomicalPart` por estrutura que possa ser isolada ou rotulada.
5. Criar um `AnatomyModelDefinition`, preencher metadados e dimensões reais e marcar como validado somente após revisão.
6. Usar o mesmo modelo para visualização AR e preparar arquivo de impressão com parâmetros e validação próprios.

## Cena de demonstração

Abra `Assets/Scenes/AnatomyLab.unity`. Se necessário, use `Digital Bones > Create Demo Scene`. A cena usa primitivas para representar um fêmur simplificado. Elas são placeholders didáticos, não modelos anatômicos de precisão.

No desktop, clique para selecionar, arraste com o botão direito para girar e use a roda do mouse para escalar. `Space` ativa a separação, `I` isola a estrutura selecionada, `R` restaura e `1` a `3` selecionam as três regiões.

Com XR ativo, a câmera acompanha a pose do headset. O gatilho direito seleciona uma estrutura, o botão A alterna a separação, o botão X isola a seleção e o analógico esquerdo gira o modelo.

## Roadmap técnico

- **MVP:** completar catálogo e troca de modelos, rótulos em cena, escala 1:1 a partir das dimensões físicas, interação XR validada e teste de usabilidade.
- **Fase 2:** associação por QR/marcador, quiz, busca e favoritos, corte/seção, headset com hand tracking e suporte offline.
- **Fase 3:** planejamento cirúrgico, aula multiusuário, importação de DICOM em fluxo controlado, painel docente e métricas de aprendizagem.

O OpenXR está instalado, mas não habilita sozinho passthrough de realidade mista no Quest. A integração Meta XR, o loader Android e os perfis de controle ainda precisam ser configurados e validados para o dispositivo alvo.

Editor usado: Unity `6000.3.27f2`, com Android Build Support, Android SDK/NDK e OpenJDK.
