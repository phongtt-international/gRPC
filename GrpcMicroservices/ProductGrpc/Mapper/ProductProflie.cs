using AutoMapper;
using Google.Protobuf.WellKnownTypes;
using ProductGrpc.Protos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ProductGrpc.Mapper
{
    public class ProductProflie : Profile
    {
        public ProductProflie()
        {
            CreateMap<Models.Product, ProductModel>()
                .ForMember(dest => dest.CreatedTime, 
                    opt =>opt.MapFrom(src => Timestamp.FromDateTime(src.CreatedTime)));
            CreateMap<ProductModel, Models.Product>()
                .ForMember(dest => dest.CreatedTime,
                    opt => opt.MapFrom(src => src.CreatedTime.ToDateTime()));
        }
    }
}
