// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
// See the License for the specific language governing permissions and
// limitations under the License.

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Messaging;
using Digitavox.Helpers;
using Digitavox.Models;

namespace Digitavox.ViewModels
{
    public partial class TutorialViewModel : ObservableObject, IOnPageKeyPress
    {
        List<string> pressedKeys = new List<string>();
        List<string> pageKeyCodes;
        [ObservableProperty]
        private FormattedString pageFormattedLabel;
        [ObservableProperty]
        private string _pageLabel;
        [ObservableProperty]
        private double _textSize;
        private DVViewModelSpeak dVViewModelSpeak;
        private FingerMapping fingerMapping;
        private UserProgress userProgress;
        private DVViewModelFunctions dVViewModelFunctions;
        public TutorialViewModel(DVViewModelSpeak dVViewModelSpeak,
                             FingerMapping fingerMapping,
                             UserProgress userProgress,
                             DVViewModelFunctions dVViewModelFunctions)
        {
            this.dVViewModelSpeak = dVViewModelSpeak;
            this.fingerMapping = fingerMapping;
            this.userProgress = userProgress;
            this.dVViewModelFunctions = dVViewModelFunctions;
        }
        public void OnPage()
        {
            dVViewModelFunctions.SetCurrentPageIdentifier("na tela de instruções de uso");
            List<string> tutorialText = new List<string>();
            List<string> tutorialSpeak = new List<string>();
            if (userProgress.UserLogged())
            {
                pageKeyCodes = new List<string>()
                {
                    "Escape"
                };
            }
            Thread.Sleep(100);
            if (DVDevice.IsAndroid())
            {
                tutorialText = new List<string>()
                {
                    "Digitavox USP iniciado.",
                    "Para uma melhor experiência na usabilidade do aplicativo, seguem algumas instruções de uso. Note que esta mensagem é exibida automaticamente apenas no primeiro acesso, mas poderá ser escutada novamente ao selecionar o item 5 do menu inicial.",
                    "Para que o Digitavox USP funcione adequadamente, o aplicativo deve ser iniciado com teclado previamente conectado e com idioma \"Brasileiro\" configurado para o teclado físico. Para configurar o idioma, o caminho a ser seguido é:",
                    "- Configurações",
                    "- Sistema",
                    "- Idiomas e entrada",
                    "- Teclado físico",
                    "- \"Nome do seu teclado\"",
                    "- Configurar os layouts de teclado",
                    "- Brasileiro",
                    "O Digitavox USP foi projetado para uso simultâneo com Talkback. Para seu funcionamento ideal, é sugerido a tecla de Pesquisa seja definida como tecla modificadora.",
                    "É importante saber também atalhos e configurações do aplicativo para facilitar a navegação. Estes são:",
                    "- Barra de espaço repete a fala das telas de menu",
                    "- Apertar qualquer tecla durante a fala do programa interrompe e pula a fala",
                    "- As instruções de navegação podem ser desabilitadas no menu de configurações",
                    "- Seta para esquerda fala a apresentação do curso enquanto na lista de cursos e apresentação da lição na lista de lições",
                    "As instruções de uso foram finalizadas. O Digitavox USP iniciará normalmente agora."
                };
                tutorialSpeak = new List<string>()
                {
                    "Digitavóx USPe iniciado.",
                    "Para uma melhor experiência na usabilidade do aplicativo, seguem algumas instruções de uso. Note que esta mensagem é exibida automaticamente apenas no primeiro acesso, mas poderá ser escutada novamente ao selecionar o item 5 do menu inicial.",
                    "Para que o Digitavóx USPe funcione adequadamente, o aplicativo deve ser iniciado com teclado previamente conectado e com idioma \"Brasileiro\" configurado para o teclado físico. Para configurar o idioma, o caminho a ser seguido é:",
                    "Configurações",
                    "Sistema",
                    "Idiomas e entrada",
                    "Teclado físico",
                    "Nome do seu teclado",
                    "Configurar os leiautes de teclado",
                    "Brasileiro",
                    "O Digitavóx USPe foi projetado para uso simultâneo com Tálquibéqui. Para seu funcionamento ideal, é sugerido que a tecla de Pesquisa seja definida como tecla modificadora.",
                    "É importante saber também atalhos e configurações do aplicativo para facilitar a navegação. Estes são:",
                    "Barra de espaço repete a fala das telas de menu",
                    "Apertar qualquer tecla durante a fala do programa interrompe e pula a fala",
                    "As instruções de navegação podem ser desabilitadas no menu de configurações",
                    "Seta para esquerda fala a apresentação do curso enquanto na lista de cursos e apresentação da lição na lista de lições",
                    "As instruções de uso foram finalizadas. O Digitavóx USP iniciará normalmente agora."
                };
            }
            else if (DVDevice.IsIos())
            {
                tutorialText = new List<string>()
                {
                    "Olá, você iniciou o Digitavox USP.",
                    "Para uma melhor experiência na usabilidade do aplicativo, você ouvirá agora algumas instruções de uso. Note que esta mensagem é exibida automaticamente apenas no primeiro acesso, mas poderá ser escutada novamente ao selecionar o item 5 do menu inicial.",
                    "O Digitavox USP foi projetado para uso simultâneo com o VoiceOver. Para seu funcionamento, é necessário configurar as teclas das setas e ESCAPE para uso pelo aplicativo, isso pode ser feito pressionando-se as setas para direita e esquerda ao mesmo tempo com o VoiceOver ativado. Para voltar as teclas de setas e ESCAPE para o controle do VoiceOver basta pressionar as setas para direita e esquerda ao mesmo tempo novamente.",
                    "É importante saber também atalhos e configurações do aplicativo para facilitar a navegação. Estes são:",
                    "- Barra de espaço repete a fala das telas de menu",
                    "- Apertar qualquer tecla durante a fala do programa interrompe e pula a fala",
                    "- As instruções de navegação podem ser desabilitadas no menu de configurações",
                    "- Seta para esquerda fala a apresentação do curso enquanto na lista de lições e apresentação da lição durante os exercícios",
                    "- Pressionar [ESC] para retornar para a tela anterior",
                    "As instruções de uso foram finalizadas. O Digitavox USP iniciará normalmente agora."
                };
                tutorialSpeak = new List<string>()
                {
                    "Olá, você iniciou o Digitavóx uspe.",
                    "Para uma melhor experiência na usabilidade do aplicativo, você ouvirá agora algumas instruções de uso. Note que esta mensagem é exibida automaticamente apenas no primeiro acesso, mas poderá ser escutada novamente ao selecionar o item 5 do menu inicial.",
                    "O Digitavox uspe foi projetado para uso simultâneo com o Vóice ôver. Para seu funcionamento, é necessário configurar as teclas das setas e esqueipe para uso pelo aplicativo, isso pode ser feito pressionando-se as setas para direita e  esquerda ao mesmo tempo com o vóice ôver ativado. Para voltar as teclas de setas e esqueipe para o controle do vóice ôver basta pressionar as setas para direita e esquerda ao mesmo tempo novamente.",
                    "É importante saber também atalhos e configurações do aplicativo para facilitar a navegação. Estes são:",
                    "Barra de espaço repete a fala das telas de menu",
                    "Apertar qualquer tecla durante a fala do programa interrompe e pula a fala",
                    "As instruções de navegação podem ser desabilitadas no menu de configurações",
                    "Seta para esquerda fala a apresentação do curso enquanto na lista de lições e apresentação da lição durante os exercícios",
                    "Pressionar esqueipe para retornar para a tela anterior",
                    "As instruções de uso foram finalizadas. O Digitavóx uspe iniciará normalmente agora."
                };
            }
            else if (DVDevice.IsMac())
            {
                tutorialText = new List<string>()
                {
                    "Olá, você iniciou o Digitavox USP.",
                    "Para uma melhor experiência na usabilidade do aplicativo, você ouvirá agora algumas instruções de uso. Note que esta mensagem é exibida automaticamente apenas no primeiro acesso, mas poderá ser escutada novamente ao selecionar o item 5 do menu inicial.",
                    "O Digitavox USP foi projetado para uso simultâneo com o VoiceOver.",
                    "É importante saber também atalhos e configurações do aplicativo para facilitar a navegação. Estes são:",
                    "- Barra de espaço repete a fala das telas de menu",
                    "- Apertar qualquer tecla durante a fala do programa interrompe e pula a fala",
                    "- As instruções de navegação podem ser desabilitadas no menu de configurações",
                    "- Seta para esquerda fala a apresentação do curso enquanto na lista de lições e apresentação da lição durante os exercícios",
                    "- Pressionar [ESC] para retornar para a tela anterior",
                    "As instruções de uso foram finalizadas. O Digitavox USP iniciará normalmente agora."
                };
                tutorialSpeak = new List<string>()
                {
                    "Olá, você iniciou o Digitavóx uspe.",
                    "Para uma melhor experiência na usabilidade do aplicativo, você ouvirá agora algumas instruções de uso. Note que esta mensagem é exibida automaticamente apenas no primeiro acesso, mas poderá ser escutada novamente ao selecionar o item 5 do menu inicial.",
                    "O Digitavox uspe foi projetado para uso simultâneo com o Vóice ôver.",
                    "É importante saber também atalhos e configurações do aplicativo para facilitar a navegação. Estes são:",
                    "Barra de espaço repete a fala das telas de menu",
                    "Apertar qualquer tecla durante a fala do programa interrompe e pula a fala",
                    "As instruções de navegação podem ser desabilitadas no menu de configurações",
                    "Seta para esquerda fala a apresentação do curso enquanto na lista de lições e apresentação da lição durante os exercícios",
                    "Pressionar esqueipe para retornar para a tela anterior",
                    "As instruções de uso foram finalizadas. O Digitavóx uspe iniciará normalmente agora."
                };
            }
            else if (DVDevice.IsWindows())
            {
                tutorialText = new List<string>()
                {
                    "Olá, você iniciou o Digitavox USP.",
                    "Para uma melhor experiência na usabilidade do aplicativo, você ouvirá agora algumas instruções de uso. Note que esta mensagem é exibida automaticamente apenas no primeiro acesso, mas poderá ser escutada novamente ao selecionar o item 5 do menu inicial.",
                    "Para o funcionamento do Digitavox USP é necessário desabilitar o leitor de tela, o que pode ser feito pressionando-se as teclas de atalho configuradas no seu assistente, como o atalho padrão tecla modificadora do NVDA pressionada juntamente com a letra Q.",
                    "É importante saber também atalhos e configurações do aplicativo para facilitar a navegação. Estes são:",
                    "- Barra de espaço repete a fala das telas de menu",
                    "- Apertar qualquer tecla durante a fala do programa interrompe e pula a fala",
                    "- As instruções de navegação podem ser desabilitadas no menu de configurações",
                    "- Seta para esquerda fala a apresentação do curso enquanto na lista de lições e apresentação da lição durante os exercícios",
                    "Parabéns, você chegou ao final das instruções de uso, o Digitavox USP iniciará normalmente agora."
                };
                tutorialSpeak = new List<string>()
                {
                    "Olá, você iniciou o Digitavóx uspe.",
                    "Para uma melhor experiência na usabilidade do aplicativo, você ouvirá agora algumas instruções de uso. Note que esta mensagem é exibida automaticamente apenas no primeiro acesso, mas poderá ser escutada novamente ao selecionar o item 5 do menu inicial.",
                    "Para o funcionamento do Digitavóx uspe é necessário desabilitar o leitor de tela, o que pode ser feito pressionando-se as teclas de atalho configuradas no seu assistente, como o atalho padrão tecla modificadora do NVDA pressionada juntamente com a letra Q.",
                    "É importante saber também atalhos e configurações do aplicativo para facilitar a navegação. Estes são:",
                    "Barra de espaço repete a fala das telas de menu",
                    "Apertar qualquer tecla durante a fala do programa interrompe e pula a fala",
                    "As instruções de navegação podem ser desabilitadas no menu de configurações",
                    "Seta para esquerda fala a apresentação do curso enquanto na lista de lições e apresentação da lição durante os exercícios",
                    "Parabéns, você chegou ao final das instruções de uso, o Digitavóx uspe iniciará normalmente agora."
                };
            }


            int penultimoIndex = tutorialText.Count - 2;
            tutorialText[penultimoIndex] = dVViewModelFunctions.EditStringForVoiceOver(tutorialText[penultimoIndex]);
            tutorialSpeak[penultimoIndex] = dVViewModelFunctions.EditStringForVoiceOver(tutorialSpeak[penultimoIndex]);

            dVViewModelSpeak.SetTextAndSpeech(tutorialText, tutorialSpeak).RegisterUpdateScreen((text) =>
            {
                
                
                PageFormattedLabel = text;
                TextSize = DVPersistence.Get<double>("fontSize");
            });
            dVViewModelSpeak.SpeakAll(() =>
            {
                if (!userProgress.UserLogged() && !dVViewModelFunctions.OnAlert())
                {
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        GoToLoginPage();
                    });
                }
            });
            WeakReferenceMessenger.Default.Send(new DVMessage("BecomeFirstResponder"));
        }
        private async void GoToLoginPage()
        {
            WeakReferenceMessenger.Default.Send(new DVMessage("CheckForScreenReader"));
            await Shell.Current.GoToAsync("Login");
        }
        public bool OnPageKeyDown(int keyCode)
        {
            string code = fingerMapping.mapKeyCode(keyCode);
            if (!pressedKeys.Contains(code))
            {
                pressedKeys.Add(code);
            }
            return true;
        }
        public bool OnPageKeyPress(int keyCode, int modifiers)
        {
            pressedKeys.Remove(fingerMapping.mapKeyCode(keyCode));
            var bean = fingerMapping.MapKey(keyCode, modifiers, pressedKeys);
            if (bean.code != null && userProgress.UserLogged())
            {
                dVViewModelSpeak.Skip();
                if (bean.code == " ")
                {
                    OnPage();
                }
                else if (pageKeyCodes.Contains(bean.code) || (bean.code == "!" && DVDevice.IsVirtual()))
                {
                    dVViewModelFunctions.HandleKeyCode(bean.code);
                }
            }
            return true;
        }
    }
}
