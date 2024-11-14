import argparse
from PIL import(
    Image 
)

def execute(args):
    image = Image.open(args.ori)
    x,y = image.size
    y2 = int(1920 / x * y)
    out = image.resize((1920, y2))
    out.save( args.dest)


if __name__ == '__main__':
    parser = argparse.ArgumentParser(formatter_class=argparse.RawTextHelpFormatter) 
    parser.add_argument('-o', dest='ori', help='ori')
    parser.add_argument('-d', dest='dest', help='dest')
    args = parser.parse_args()
    execute(args)
