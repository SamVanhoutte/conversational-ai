import nltk
#nltk.download('averaged_perceptron_tagger')
#nltk.download('punkt')
sentence = "I want to book a flight for 4 persons from New York to Amsterdam"
tokens =nltk.word_tokenize(sentence)
print(tokens)
tagged_tokens = nltk.pos_tag(tokens)
for tagged_token in tagged_tokens:
    print(tagged_token[0].rjust(15), tagged_token[1])