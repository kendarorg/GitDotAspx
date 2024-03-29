#region License

// Copyright 2010 Jeremy Skinner (http://www.jeremyskinner.co.uk)
//  
// Licensed under the Apache License, Version 2.0 (the "License"); 
// you may not use this file except in compliance with the License. 
// You may obtain a copy of the License at 
// 
// http://www.apache.org/licenses/LICENSE-2.0 
// 
// Unless required by applicable law or agreed to in writing, software 
// distributed under the License is distributed on an "AS IS" BASIS, 
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied. 
// See the License for the specific language governing permissions and 
// limitations under the License.
// 
// The latest version of this file can be found at http://github.com/JeremySkinner/git-dot-aspx

#endregion

namespace GitAspx.Lib {
	using System;
	using System.Web.Mvc;
	using System.Web.Routing;
	using GitAspx.Controllers;
	using StructureMap;

	public class StructureMapControllerFactory : DefaultControllerFactory {
		protected override IController GetControllerInstance(RequestContext requestContext, Type controllerType) {
			var controller = (IController) ObjectFactory.GetInstance(controllerType);

			var baseCon = controller as BaseController;
			if (baseCon != null) {
				baseCon.AppSettings = ObjectFactory.GetInstance<AppSettings>();
			}

			return controller;
		}
	}
}