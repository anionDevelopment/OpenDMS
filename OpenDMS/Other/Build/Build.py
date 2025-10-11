from ScriptCollection.TFCPS.Docker.TFCPS_CodeUnitSpecific_Docker import TFCPS_CodeUnitSpecific_Docker_Functions,TFCPS_CodeUnitSpecific_Docker_CLI

 
def build():
    tf:TFCPS_CodeUnitSpecific_Docker_Functions=TFCPS_CodeUnitSpecific_Docker_CLI.parse(__file__)
    tf.build()
    tf.tfcps_Tools_General.merge_sbom_file_from_dependent_codeunit_into_this(tf.get_codeunit_folder(),tf.get_codeunit_name(),"OpenDMSBackend",tf.use_cache())
    tf.tfcps_Tools_General.merge_sbom_file_from_dependent_codeunit_into_this(tf.get_codeunit_folder(),tf.get_codeunit_name(),"OpenDMSFrontend",tf.use_cache())


if __name__ == "__main__":
    build()
