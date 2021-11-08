from nltk import word_tokenize, pos_tag, ne_chunk
from nltk.stem import PorterStemmer 
from nltk.tag import untag, str2tuple, tuple2str
from nltk.chunk import tree2conllstr, conllstr2tree, conlltags2tree, tree2conlltags

def print_tuples(tuple_list: list, title: str):
    print("==============================")
    print(title)
    for tup in tuple_list:
        print(tup[0].rjust(15), " - ", tup[1])
    print("==============================")

def print_trees(tree_list, title:str, level: int = 1):
    if(title):
        print("==============================")
        print(title)
    for node in tree_list:
        if type(node) is nltk.Tree:
            if node.label() == 'ROOT':
                print("-" * level, "Sentence:", " ".join(node.leaves()))
            else:
                
                print("-" * (level), "Label:", node.label())
                print(" " * (level) , "Leaves:", node.leaves())

            print_trees(node, None, level + 1)
        else:
            print("-" * level, "Word:", node)
    if(title):
        print("==============================")


import nltk


# Two dictionary downloads (only first time)
nltk.download('maxent_ne_chunker')
nltk.download('words')
 
text = "John works at Intel."
text = "I saw a man on a hill with a telescope"
text = "I am selling flowers in malls"
 
tokens = word_tokenize(text)
ps = PorterStemmer() 
print("==============================")
print("STEMMING")
for w in tokens: 
    print(w.rjust(15), " - ", ps.stem(w)) 

tagged_tokens = pos_tag(tokens)
print()
print_tuples( tagged_tokens , "POS TAGGING") 
 
ner_tree = ne_chunk(tagged_tokens)
#print_trees( ner_tree , "NER TREE") 

iob_tagged = tree2conlltags(ner_tree)
print_trees( iob_tagged , "IOB TAGGED") 

ner_tree = conlltags2tree(iob_tagged)
#print_trees( ner_tree , "TAGS TREE") 

tree_str = tree2conllstr(ner_tree)
ner_tree = conllstr2tree(tree_str, chunk_types=('PERSON', 'ORGANIZATION'))

