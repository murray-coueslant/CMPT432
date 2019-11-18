namespace Illumi_CLI {
    class TypeChecker {
        public AbstractSyntaxTree Tree { get; set; }
        public TypeChecker (AbstractSyntaxTree tree) {
            Tree = tree;
        }

        public void TypeCheck () {
            return;
        }
    }
}