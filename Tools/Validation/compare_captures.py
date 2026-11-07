"""Compare two RGB/RGBA PNG captures without third-party dependencies."""
import struct
import sys
import zlib


def pixels(path):
    raw = open(path, 'rb').read()
    assert raw[:8] == b'\x89PNG\r\n\x1a\n'
    offset, compressed = 8, bytearray()
    while offset < len(raw):
        length = struct.unpack('>I', raw[offset:offset+4])[0]
        kind, data = raw[offset+4:offset+8], raw[offset+8:offset+8+length]
        offset += length + 12
        if kind == b'IHDR':
            width, height, depth, color, _, _, interlace = struct.unpack('>IIBBBBB', data)
            assert depth == 8 and color in (2, 6) and not interlace
        elif kind == b'IDAT':
            compressed.extend(data)
    channels = 3 if color == 2 else 4
    stride, stream = width * channels, zlib.decompress(compressed)
    result, previous, offset = [], bytearray(width * channels), 0
    for _ in range(height):
        mode = stream[offset]
        row = bytearray(stream[offset+1:offset+1+stride])
        offset += stride + 1
        for i in range(stride):
            left = row[i-channels] if i >= channels else 0
            above = previous[i]
            upper_left = previous[i-channels] if i >= channels else 0
            if mode == 1:
                prediction = left
            elif mode == 2:
                prediction = above
            elif mode == 3:
                prediction = (left + above) // 2
            elif mode == 4:
                p = left + above - upper_left
                distances = (abs(p-left), abs(p-above), abs(p-upper_left))
                prediction = (left, above, upper_left)[distances.index(min(distances))]
            else:
                assert mode == 0
                prediction = 0
            row[i] = (row[i] + prediction) & 255
        result.extend(tuple(row[i:i+3]) for i in range(0, stride, channels))
        previous = row
    return (width, height), result


if __name__ == '__main__':
    size_a, a = pixels(sys.argv[1])
    size_b, b = pixels(sys.argv[2])
    assert size_a == size_b
    means = [sum(abs(x[c]-y[c]) for x, y in zip(a, b))/len(a) for c in range(3)]
    print(f'{size_a[0]}x{size_a[1]}: mean RGB absolute delta = {means}')
    assert max(means) >= 1, 'Color-grade effect is too small to verify'
