from ScriptCollection.TFCPS.NodeJS.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI



def build():
    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)
    tf.build()
    tf.add_culture_chooser(tf.get_product_name(),tf.get_available_cultures_for_angular_app())
    tf.add_maintenance_site(tf.get_product_name())
    tf.organize_translations(["ar","cs","da","de","de-CH","de-AT","en-GB","es","fa","fi","fr","he","hi","id","it","ja","ko","ms","nl","ms-MY","ms-SG","nb","pl","pt","pt-BR","ru","sv","th","ur","vi","zh"])


if __name__ == "__main__":
    build()
