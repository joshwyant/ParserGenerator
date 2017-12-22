//using System;
//using System.Collections.Generic;
//using System.Data.Common;
//using System.Linq;
//
//namespace ParserGenerator
//{
//    public class GraphStructuredStack<TValue, TLinkValue> 
//        where TLinkValue : class 
//    {
//        private readonly HashSet<Path> _paths = new HashSet<Path>();
//        public IReadOnlyCollection<Path> Paths => _paths;
//        
//        public GraphStructuredStack(TValue value)
//        {
//            var initialPath = new Path(this);
//            initialPath.Push(initialItem.value, initialItem.linkValue);
//            _paths.Add(initialPath);
//        }
//
//        void RecalculateDepths()
//        {
//            var visitedSet = new HashSet<Node>(_paths.Select(p => p._top));
//            var traversalQueue = new Queue<Node>(visitedSet);
//            var sortedStack = new Stack<Node>();
//
//            while (traversalQueue.Count > 0)
//            {
//                var n = traversalQueue.Dequeue();
//                visitedSet.Add(n);
//                sortedStack.Push(n);
//                foreach (var parent in n.Links.Select(l => l.Parent).Where(p => !visitedSet.Contains(p)))
//                {
//                    traversalQueue.Enqueue(parent);
//                }
//            }
//
//            foreach (var n in sortedStack)
//            {
//                if (n.Links.Count == 1) // The normal case
//                {
//                    n.DeterministicDepth = n.Links.Single().Parent.DeterministicDepth + 1;
//                }
//                else if (n.Links.Any()) // Multiple links
//                {
//                    n.DeterministicDepth = 0;
//                }
//                else // The bottom of the stack
//                {
//                    n.DeterministicDepth = 1;
//                }
//            }
//        }
//
//        internal class Link
//        {
//            public Node Parent { get; }
//            public TLinkValue SemanticValue { get; }
//            
//            internal Link(Node parent, TLinkValue semanticValue)
//            {
//                Parent = parent;
//                SemanticValue = semanticValue;
//            }
//        }
//        
//        internal class Node
//        {
//            /// <summary>
//            /// Used for creating the initial node in the stack only.
//            /// </summary>
//            internal Node(TValue value)
//            {
//                Value = value;
//                DeterministicDepth = 1;
//            }
//            
//            internal Node(Node parent, TValue value, TLinkValue semanticValue)
//            {
//                if (parent == null) throw new ArgumentNullException("parent");
//                _links.Add(new Link(parent, semanticValue));
//                Value = value;
//                DeterministicDepth = parent.DeterministicDepth + 1;
//            }
//            
//            private readonly List<Link> _links = new List<Link>();
//            public IReadOnlyCollection<Link> Links => _links;
//            public TValue Value { get; }
//            public int DeterministicDepth { get; internal set; }
//
//            public override int GetHashCode() => Value.GetHashCode();
//            public override bool Equals(object obj) => ReferenceEquals(this, obj);
//        }
//
//        public class Path
//        {
//            public int DeterministicDepth => _top?.DeterministicDepth ?? 0;
//            
//            /// <summary>
//            /// Used for creating the initial path only.
//            /// </summary>
//            /// <param name="gss">The graph-structured stack this path belongs to.</param>
//            /// <param name="initialValue">The value of the initial state.</param>
//            internal Path(GraphStructuredStack<TValue, TLinkValue> gss, TValue initialValue)
//            {
//                _gss = gss;
//                _top = new Node(initialValue);
//            }
//
//            private Path(GraphStructuredStack<TValue, TLinkValue> gss, Node top)
//            {
//                _gss = gss;
//                _top = top;
//            }
//
//            private readonly GraphStructuredStack<TValue, TLinkValue> _gss;
//
//            internal Node _top;
//
//            public void Push(TValue value, TLinkValue semanticValue = null)
//            {
//                var n = new Node(_top, value, semanticValue);
//                _top = n;
//            }
//
//            public IEnumerable<Path> Push(IEnumerable<(TValue value, TLinkValue semanticValue)> values)
//            {
//                var top = _top;
//                var i = 0;
//                foreach (var t in values)
//                {
//                    var node = new Node(top, t.value, t.semanticValue);
//                    if (i++ == 0)
//                    {
//                        _top = node;
//                        yield return this;
//                    }
//                    else
//                    {
//                        var p = new Path(_gss, node);
//                        _gss._paths.Add(p);
//                        yield return p;
//                    }
//                }
//            }
//
//            public IEnumerable<Path> Push(params (TValue value, TLinkValue semanticValue)[] values)
//            {
//                return Push(values as IEnumerable<(TValue value, TLinkValue semanticValue)>);
//            }
//
//            public TLinkValue DeterministicPop()
//            {
//                if (DeterministicDepth < 1)
//                {
//                    throw new InvalidOperationException();
//                }
//
//                var link = _top.Links.SingleOrDefault();
//                _top = link.Parent;
//                return link.SemanticValue;
//            }
//            
//            public IEnumerable<TLinkValue> DeterministicPopN(int n)
//            {
//                if (n > DeterministicDepth)
//                {
//                    throw new InvalidOperationException();
//                }
//
//                while (n-- > 0)
//                {
//                    yield return DeterministicPop();   
//                }
//            }
//
//            public TValue Peek()
//            {
//                return _top.Value;
//            }
//
//            public override int GetHashCode()
//            {
//                return _top?.GetHashCode()??0;
//            }
//
//            public override bool Equals(object obj)
//            {
//                return obj is Path p && p._top.Value.Equals(_top.Value);
//            }
//        }
//    }
//}