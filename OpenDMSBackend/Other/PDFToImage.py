#!/usr/bin/env python3
import os
import argparse
import fitz  # PyMuPDF
from PIL import Image


class PDFToImage:

    def __pdf_to_image(self, input_file: str, output_file: str, dpi=150, all_pages=False):
        """
        Convert PDF pages to a single image file.

        :param input_file: Path to the PDF file
        :param output_file: Path to the output image (png/jpg)
        :param dpi: Resolution in DPI
        :param all_pages: If True, concatenate all pages vertically
        """
        doc = fitz.open(input_file)
        images = []

        for page_index in range(doc.page_count):
            pix = doc[page_index].get_pixmap(dpi=dpi)
            # Convert pixmap to PIL Image
            img = Image.frombytes("RGB", [pix.width, pix.height], pix.samples)
            images.append(img)
            if not all_pages:
                break  # Only first page

        if all_pages and len(images) > 1:
            # Concatenate vertically
            total_height = sum(img.height for img in images)
            max_width = max(img.width for img in images)
            concatenated = Image.new("RGB", (max_width, total_height), color=(255, 255, 255))
            y_offset = 0
            for img in images:
                concatenated.paste(img, (0, y_offset))
                y_offset += img.height
            concatenated.save(output_file)
            print(f"All pages of '{input_file}' concatenated and saved as '{output_file}'")
        else:
            images[0].save(output_file)
            print(f"Page 1 of '{input_file}' saved as '{output_file}'")

    def main(self):
        parser = argparse.ArgumentParser(description="Convert PDF to image")
        parser.add_argument("-i", "--input", required=True, help="Input PDF file")
        parser.add_argument("-o", "--output", required=True, help="Output image file (png/jpg)")
        parser.add_argument("--dpi", type=int, default=150, help="Resolution in DPI (default: 150)")
        parser.add_argument("--all", action="store_true", help="Concatenate all pages into one image")

        args = parser.parse_args()

        if not os.path.isfile(args.input):
            raise ValueError(f"Error: Input file '{args.input}' does not exist.")

        self.__pdf_to_image(args.input, args.output, dpi=args.dpi, all_pages=args.all)


def run():
    pdfToImage: PDFToImage = PDFToImage()
    pdfToImage.main()


if __name__ == "__main__":
    run()
