import nltk
#nltk.download('averaged_perceptron_tagger')
#nltk.download('punkt')
sentence = "I am selling flowers in the mall"
tokens =nltk.word_tokenize(sentence)
print(tokens)
tagged_tokens = nltk.pos_tag(tokens)
for tagged_token in tagged_tokens:
    print(tagged_token[0].rjust(15), tagged_token[1])


