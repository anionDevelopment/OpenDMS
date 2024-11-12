import os
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def run_test_service():
    TasksForCommonProjectStructure().run_local_test_service(os.path.abspath(__file__))

if __name__ == "__main__":
    run_test_service()
