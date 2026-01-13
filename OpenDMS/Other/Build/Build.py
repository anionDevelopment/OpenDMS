from pathlib import Path
from packaging.version import Version
from ScriptCollection.GeneralUtilities import GeneralUtilities
from ScriptCollection.ImageUpdater import ConcreteImageUpdaterForDebian
from ScriptCollection.TFCPS.Docker.TFCPS_CodeUnitSpecific_Docker import TFCPS_CodeUnitSpecific_Docker_Functions,TFCPS_CodeUnitSpecific_Docker_CLI

 
def build():
    tf:TFCPS_CodeUnitSpecific_Docker_Functions=TFCPS_CodeUnitSpecific_Docker_CLI.parse(__file__)
    debian_tag:str=GeneralUtilities.read_text_from_file_without_linebreak(GeneralUtilities.resolve_relative_path("../../../../Other/Resources/Dependencies/Debian/Version.txt",str(Path(__file__).absolute())))
    tf.build({
        "debianversion":debian_tag,
    },{
        "debian":tf._protected_sc.default_fallback_docker_registry,
    })
    tf.tfcps_Tools_General.merge_sbom_file_from_dependent_codeunit_into_this(tf.get_codeunit_folder(),tf.get_codeunit_name(),"ConSurvBackend",tf.use_cache())
    tf.tfcps_Tools_General.merge_sbom_file_from_dependent_codeunit_into_this(tf.get_codeunit_folder(),tf.get_codeunit_name(),"ConSurvFrontend",tf.use_cache())


if __name__ == "__main__":
    build()
