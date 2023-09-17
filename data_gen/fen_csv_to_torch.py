import csv
import torch

fen_letter_to_idx = {"K":(5,0),"Q":(4,0),"R":(3,0),"B":(2,0),"N":(1,0),"P":(0,0),"k":(5,1),"q":(4,1),"r":(3,1),"b":(2,1),"n":(1,1),"p":(0,1)}
fen_letter_to_idx["b"]
numbers = "12345678"
figures = "KQRBNPkqrbnp"

def fen_to_array(fen):
    #a = torch.zeros((8,8,6,2))
    a = torch.zeros((2,6,8,8))
    letter_idx = 7
    number_idx = 0
    for i in fen:
        if i == '/':
            letter_idx -= 1
            number_idx = 0
        if i in numbers:
            number_idx += int(i)
        if i in figures:
            # print(i,letter_idx,number_idx,fen_letter_to_idx[i][0],fen_letter_to_idx[i][1], fen)
            #a[letter_idx,number_idx,fen_letter_to_idx[i][0],fen_letter_to_idx[i][1]] = 1
            a[fen_letter_to_idx[i][1],fen_letter_to_idx[i][0],letter_idx,number_idx] = 1
            number_idx += 1
        if i == " ":
            break
    return a

input_data = []
output_data = []

with open('fen4.csv', newline='') as csvfile:

    csvreader = csv.reader(csvfile, delimiter=',')

    for i, row in enumerate(csvreader):
        print(i)

        if(row[0] == ''):
            continue
 
        value = int(row[0])
        fen = row[2]

        fen_array = fen_to_array(fen)

        # print(value, fen, print(fen_array[:,:,5,0]-fen_array[:,:,5,1]))

        if(fen.split(" ")[1] == 'w'):
            value = -value
            fen_array = fen_array.flip(0)
            fen_array = fen_array.flip(3)

            # print(value, fen, print(fen_array[:,:,5,0]-fen_array[:,:,5,1]))

        # print("="*10)

        input_data.append(fen_array)
        output_data.append(value)

input_data = torch.stack(input_data)
output_data = torch.tensor(output_data)

print(input_data.shape)
print(output_data.shape)

torch.save(input_data, 'input_data.pt')
torch.save(output_data, 'output_data.pt')