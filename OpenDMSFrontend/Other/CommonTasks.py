from ScriptCollection.TFCPS.NodeJS.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI


def common_tasks():
    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)    
    tf.do_common_tasks(tf.get_version_of_project())#codeunit-version should alsways be the same as project-version
    tf.tfcps_Tools_General.generate_api_client_from_dependent_codeunit_for_angular(tf.get_codeunit_folder(),"OpenDMSBackend","open-dms-backend", "typescript-angular",tf.use_cache())
 
if __name__ == "__main__":
    common_tasks()
 