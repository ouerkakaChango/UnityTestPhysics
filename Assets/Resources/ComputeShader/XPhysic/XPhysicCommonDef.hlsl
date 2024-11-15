#ifndef XPHYSICCOMMONDEF_HLSL
#define XPHYSICCOMMONDEF_HLSL

uint2 GetSize(RWTexture2D<float4> dst)
{
    uint2 size;
    dst.GetDimensions(size.x, size.y);
    return size;
}

uint GetIDFromIxy(uint2 ixy, uint2 size)
{
    return ixy.x + ixy.y * size.x;
}

#endif