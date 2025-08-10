from CountryInformation.Language import Language
from ScriptCollection.GeneralUtilities import GeneralUtilities
from CountryInformation.CountryInformationCore import CountryInformationCore


def print_languages():
    ci: CountryInformationCore = CountryInformationCore()
    languages: list[Language] = ci.get_all_languages()
    for language in languages:
        GeneralUtilities.write_message_to_stdout(str(language))


if __name__ == "__main__":
    print_languages()
