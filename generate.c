#include <uv.h>
#include <stdio.h>

#define UV_ERRNO_GEN(val, name, s) \
if (val != UV_UNKNOWN) { \
  printf("\t\tUV_%s = %u,\n", #name, val); \
} else { \
  printf("\t\tUV_UNKNOWN = -1,\n"); \
}


int main(int i, char **argv)
{
  printf("using System;\n\nnamespace LibuvSharp\n{\n\tpublic enum uv_err_code\n\t{\n");
  UV_ERRNO_MAP(UV_ERRNO_GEN)
  printf("\t}\n}\n");
  return 0;
}
