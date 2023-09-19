from stockfish import Stockfish
from multiprocessing import Pool
import csv

fen_list = []
with open('lifen.txt','r') as result_file:
    rows = csv.reader(result_file)
    for row in rows:
        if(len(row) == 0):
            continue
        if('[' in row[0]):
            continue
        fen = row[0]
        fen_list.append(fen)



fen_list = fen_list[:1000000]

print(len(fen_list))

def chunks(xs, m):
    n = int(len(xs) / m)+1
    return [xs[i:i+n] for i in range(0, len(xs), n)]

cores = 10

fen_list_chunks = chunks(fen_list, cores)

def stockfish_evaluate(fen_list):
    stockfish = Stockfish(path="Stockfish-sf_16/src/stockfish", depth=15, parameters={"Threads": 1, "Hash": 256}) #
    return_list = []
    for idx,fen in enumerate(fen_list):
        if(idx % 100 == 0):
            print(idx/len(fen_list))
        stockfish.set_fen_position(fen)
        eval = stockfish.get_evaluation()
        cp = None
        mate = None
        if(eval["type"] == "cp"):
            cp = eval["value"]
        elif(eval["type"] == "mate"):
            mate = eval["value"]
        else:
            print("ERROR")
            exit()
        return_list.append((cp,mate,fen))
    return return_list

with Pool(cores+1) as p:
    results_list = p.map(stockfish_evaluate, fen_list_chunks)

print(results_list)
final_list = []
for results in results_list:
    for result in results:
        final_list.append(result)

print(final_list)

with open('fen3.csv','w') as result_file:
    wr = csv.writer(result_file)
    for row in final_list:
        wr.writerow(row)