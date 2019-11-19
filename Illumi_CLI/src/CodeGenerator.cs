using System.Text;
namespace Illumi_CLI {
    class CodeGenerator {
        public SemanticAnalyser SemanticAnalyser { get; set; }
        public StringBuilder CodeString { get; set; }
        public RuntimeImage Image { get; set; }
        public CodeGenerator (SemanticAnalyser semanticAnalyser) {
            SemanticAnalyser = semanticAnalyser;
        }
        public void Generate () {

        }
    }
}