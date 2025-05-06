import os
from ScriptCollection.TasksForCommonProjectStructure import TasksForCommonProjectStructure


def start_local_test_service():
    stript_file=__file__
    TasksForCommonProjectStructure().start_local_test_service(stript_file)


if __name__ == "__main__":
    start_local_test_service()
