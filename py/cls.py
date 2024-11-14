import os
import shutil
import argparse

from ws.utils import(
    fmt4d
)

def execute(args):
    dict = {}
    with open(args.dest, 'a', encoding='utf-8') as w:
        with open(args.ori, 'r') as file:
            lines = file.readlines()
            for line in lines:
                input = line.rstrip('\n')
                if(len(input) == 64 and dict.get(input, -1) == -1):
                    w.write(input + '\n')
                    dict[input] = 1             
    
if __name__ == '__main__':
    parser = argparse.ArgumentParser(formatter_class=argparse.RawTextHelpFormatter) 
    parser.add_argument('-o', dest='ori', help='ori file')
    parser.add_argument('-d', dest='dest', help='dest file')
    args = parser.parse_args()
    execute(args)
   
