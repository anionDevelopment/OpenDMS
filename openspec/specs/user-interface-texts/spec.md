# user-interface-texts Specification

## Purpose

Keeps every translatable text of the product free of placeholders, so that a text which is handed to a translator is
a complete sentence in itself and a value which belongs next to it is put there by the code which shows it.

A text like `"{selectedParticipants} of {maximumParticipants} participants selected"` is hard to translate well: the
translator cannot see what the placeholders hold, cannot always move them where the word-order of the target language
needs them, and a language with other plural- or case-rules than english ends up with a sentence which is
grammatically wrong for some of the values. Written as a value plus a text - `2/3` followed by
`"participants selected"` - the same information is translatable as one short, complete phrase.

## Requirements

### Requirement: A translatable text contains no placeholder

Every text of the user-interface which is translated (an entry of a translation-resource, for example an ARB-,
XLIFF-, JSON- or RESX-entry) SHALL be a text without a placeholder: no named placeholder, no positional argument, no
plural- or select-construct.

A value which belongs to such a text (an amount, a name, a date, an identifier, an error) SHALL be shown next to the
text by the code which builds that part of the user-interface, so that the translated resource stays a complete
phrase of its own.

#### Scenario: A text needs to show an amount

- **WHEN** a part of the user-interface has to show how many of something there are
- **THEN** the translatable text states what is counted (for example "participants selected") and the code puts the
  amount in front of it (for example "2/3 participants selected")

#### Scenario: A text needs to show a value which is not a text of the product

- **WHEN** a part of the user-interface has to show a date, a name, an identifier or an error-message
- **THEN** the translatable text is the label of that value and the code shows the value next to it, separated the
  way the user-interface separates a label from its value

#### Scenario: A new text is added

- **WHEN** a text is added to a translation-resource
- **THEN** it contains no placeholder, and the value which belongs to it is composed in the code instead

#### Scenario: A text with a placeholder is found

- **WHEN** a translatable text which contains a placeholder exists
- **THEN** this is a defect, resolved by splitting the text into a placeholder-free text and the value which the code
  shows next to it
