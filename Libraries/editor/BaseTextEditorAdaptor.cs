﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace VDS.RDF.Utilities.Editor
{
    public abstract class BaseTextEditorAdaptor<T> : ITextEditorAdaptor<T>
    {
        private T _control;

        public BaseTextEditorAdaptor(T control)
        {
            this._control = control;
        }

        public T Control
        {
            get 
            { 
                return this._control; 
            }
        }

        public abstract string Text
        {
            get;
            set;
        }

        public abstract int TextLength
        {
            get;
        }

        public abstract int CaretOffset
        {
            get;
        }

        public abstract int SelectionStart
        {
            get;
            set;
        }

        public abstract int SelectionLength
        {
            get;
            set;
        }

        public abstract void Cut();

        public abstract void Copy();

        public abstract void Paste();

        public abstract void Undo();

        public abstract void Redo();

        public abstract void SetHighlighter(String name);
    }
}
