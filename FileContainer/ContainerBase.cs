﻿using System;
using System.Collections.Generic;
using HyoutaPluginBase.FileContainer;

namespace HyoutaTools.FileContainer {
	public abstract class ContainerBase : IContainer {
		public bool IsFile => false;
		public bool IsContainer => true;
		public IFile AsFile => null;
		public IContainer AsContainer => this;

		public abstract void Dispose();
		public abstract INode GetChildByIndex( long index );
		public abstract INode GetChildByName( string name );
		public abstract IEnumerable<string> GetChildNames();
	}
}
