import math

def cal_inratio(p, c, d):
    _all = math.comb(p, c)
    _noin = math.comb(p - d, c)
    _in = _all - _noin
    return _in / _all

array = [(316, 30), 
             (329, 65),
             (434, 18),
             (296, 30),
             (231, 50),
             (524, 40),
             (449, 20)]

def get_allin(array, t):
    p = 1
    for i in array:
        p = p * cal_inratio(i[0], i[1], t)
    return p

def get_allnotin(array, t):
    p = 1
    for i in array:
        p = p * (1 - cal_inratio(i[0], i[1], t))
    return p

def get_onein(array, t):
    return 1 - get_allnotin(array, t)

print(get_allnotin(array, 5))
