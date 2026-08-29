from ScriptCollection.TFCPS.NodeJS.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI


def common_tasks():
    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)
    tf.do_common_tasks(tf.get_version_of_project())#codeunit-version should alsways be the same as project-version
    tf.tfcps_Tools_General.generate_api_client_from_dependent_codeunit(tf.get_codeunit_folder(),"OpenDMSBackend","src/app/generated/open-dms-backend", "typescript-angular",tf.use_cache(),["apis","models","supportingFiles"])
    tf.organize_translations(["ar","cs","da","de","de-CH","de-AT","el","en-GB","es","fa","fi","fr","he","hi","id","it","ja","ko","ms","nl","ms-SG","nb","pl","pt","ru","sv","th","ur","vi","zh"])
     
if __name__ == "__main__":
    common_tasks()
 