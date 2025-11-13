
# AI Agent instructions for Zeitung repository

## Coding Guidelines
see code change guidelines .github/agents/code-changes.md

inform/alert user if rules are violated (by existing code or new code) like:
- duplicating code/functions
- multiple classes/interfaces in one file

## Testing instructions
- Find the CI plan in the .github/workflows folder.
- Fix any test or type errors until the whole suite is green.
- Add or update tests for the code you change, even if nobody asked. But keep it minimal and do not weaken existing tests.

## PR instructions
Title format: <type>(<scope>): <description>
while scope is optional
further see .github/agents/pull-requests.md

branch name should start with "agent/123-description-of-task" where 123 is the current PR id


## creating documentation
- do not create redundant documentation (like repeating something already written in code)
- do not create MD files for things u just did. use chat or other output to communicate this to the user
- keep documentation minimal since we except readers to be experienced developers


## Ending a Task/Conversation/Session
- if you did change code run related tests
- at the end scan similar files and evaluate if there is similar/duplicated code;
  - refactor those BUT only if it is safe and minimal
  - AND inform the user about it
  - AND create a own commit for each refactor (so review is easy)
