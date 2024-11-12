import sys
from pathlib import Path
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def run_example():
    file = str(Path(__file__).absolute())
    t = TasksForCommonProjectStructure()
    t.run_dockerfile_example(file, 0, True, True, sys.argv)
    # HINT now open https://<domain>:443/API/Other/Resources/APISpecification/index.html in your browser.


if __name__ == "__main__":
    run_example()
