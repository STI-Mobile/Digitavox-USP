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


using Foundation;
using Microsoft.Maui.Handlers;
using Microsoft.Maui.Platform;
using UIKit;
using Digitavox.Models;
using Digitavox.ViewModels;
using Digitavox.Helpers;
using CommunityToolkit.Mvvm.Messaging;

namespace Digitavox.PlatformsImplementations
{
    public partial class DVLayoutHandler : LayoutHandler {
    protected override LayoutView CreatePlatformView() {
      return new DVLayoutView();
    }

    protected override void ConnectHandler(LayoutView platformView) {
      platformView.UserInteractionEnabled = true;
      base.ConnectHandler(platformView);
    }
  }
}

public class DVLayoutView : LayoutView {

  WeakReferenceMessenger dvMessenger;

  public DVLayoutView() {
    this.BecomeFirstResponder();

    WeakReferenceMessenger.Default.Register<DVMessage>(this, (r, m) => {
      if (m.Value == "BecomeFirstResponder")
        this.BecomeFirstResponder();
    });
  }

  public override bool CanBecomeFirstResponder {
    get {
      return true;
    }
  }

  public override void TouchesBegan(NSSet touches, UIEvent evt) {
      this.BecomeFirstResponder();
      base.TouchesBegan(touches, evt);
  }

  public override void PressesBegan(NSSet<UIPress> presses, UIPressesEvent evt) {
    
    base.PressesBegan(presses, evt);
  }

  public override void PressesEnded(NSSet<UIPress> presses, UIPressesEvent evt) {
    
    
  }

  public override void PressesCancelled(NSSet<UIPress> presses, UIPressesEvent evt) {
    
    

    

    int keyCode = (int)presses.AnyObject.Key.KeyCode;
    int keyModifiers = ((int)presses.AnyObject.Key.ModifierFlags >> 16);


    
    
    
    
    if (keyModifiers == 0x08) 
      keyModifiers |= 0x40;   

    Console.WriteLine("keyCode: " + keyCode.ToString() + ", keyModifiers: " + keyModifiers.ToString());

    //// para não processar pressesEnd de tecla modifier
    
    
    
    

    Page p = Shell.Current.CurrentPage;
    if (p is IOnPageKeyPress) {
      bool handled = (p as IOnPageKeyPress).OnPageKeyPress(keyCode, keyModifiers);
      if (!handled)
        base.PressesCancelled(presses, evt);
    } else
      base.PressesCancelled(presses, evt);
  } 
}