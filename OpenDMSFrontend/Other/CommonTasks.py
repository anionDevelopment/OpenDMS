from ScriptCollection.TFCPS.NodeJS.TFCPS_CodeUnitSpecific_NodeJS import TFCPS_CodeUnitSpecific_NodeJS_Functions,TFCPS_CodeUnitSpecific_NodeJS_CLI


def common_tasks():
    tf:TFCPS_CodeUnitSpecific_NodeJS_Functions=TFCPS_CodeUnitSpecific_NodeJS_CLI.parse(__file__)
    tf.do_common_tasks(tf.get_version_of_project())#codeunit-version should alsways be the same as project-version
    tf.tfcps_Tools_General.generate_api_client_from_dependent_codeunit(tf.get_codeunit_folder(),"OpenDMSBackend","src/app/generated/open-dms-backend", "typescript-angular",tf.use_cache(),["apis","models","supportingFiles"])
    tf.organize_translations([
        "ar",  # Arabic
        "bn",  # Bengali
        "cs",  # Czech
        "da",  # Danish
        "de",  # German
        "de-AT",  # German (Austria)
        "de-CH",  # German (Switzerland)
        "el",  # Greek
        "en-GB",  # English (United Kingdom)
        "es",  # Spanish
        "fa",  # Persian
        "fi",  # Finnish
        "fr",  # French
        "he",  # Hebrew
        "hi",  # Hindi
        "hu",  # Hungarian
        "id",  # Indonesian
        "it",  # Italian
        "ja",  # Japanese
        "ko",  # Korean
        "ms",  # Malay
        "ms-ID",  # Malay (Indonesia)
        "ms-SG",  # Malay (Singapore)
        "nb",  # Norwegian Bokmål
        "nl",  # Dutch
        "pl",  # Polish
        "pt",  # Portuguese
        "ro",  # Romanian
        "ru",  # Russian
        "sv",  # Swedish
        "th",  # Thai
        "tr",  # Turkish
        "uk",  # Ukrainian
        "ur",  # Urdu
        "vi",  # Vietnamese
        "zh",  # Chinese
    ])
     
if __name__ == "__main__":
    common_tasks()
 