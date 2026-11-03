// Copyright 2024-2026 Universidade de São Paulo
//
// Licensed under the Apache License, Version 2.0 (the "License");
// you may not use this file except in compliance with the License.
// You may obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0

namespace Digitavox.ViewModels;

public sealed class AlertViewModel
{
    public string AlertText { get; } =
        "Para que a síntese de voz do aplicativo funcione adequadamente, é necessário estar com o Talkback desabilitado. " +
        "Para desativá-lo vá até as configurações e na seção de acessibilidade selecione para desligar ou utilize seu atalho definido. " +
        "Quando desativar, essa janela será automaticamente removida e o aplicativo passará ao seu funcionamento normal. " +
        "Caso necessário pressione espaço para repetir o texto da página e lembre-se de ligar o Talkback novamente ao sair do aplicativo.";
}
