bool IsNan(float x)
{
    return (asuint(x) & 0x7fffffff) > 0x7f800000;
}